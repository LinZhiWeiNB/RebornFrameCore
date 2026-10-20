using System;
using System.Collections.Generic;
using System.Threading;
using RebornFrame.Utils;
using UnityEngine;

namespace RebornFrame.Core
{
    public class RebornCore : Singleton<RebornCore>
    {
        public Action<int> OnFixedUpdate;
        public Action OnUpdate;
        public Action<int> OnRollBack;//需要回滚时触发，只在回滚开始时触发，回滚中会正常调用onupdate

        private long _CaculateLogicTick;
        public long CaculateLogicTick => _CaculateLogicTick;
        private int _TargetFrame = 45;
        private int _ClinetFrameRateScale = 3;
        public int ClientFrameRateScale => _ClinetFrameRateScale;
        private int logicFrameDeltaTime;
        private long lastUpdateTick;
        private long _deltaTimeTick;//累计的tick
        private int logicFrame = -1;
        public int QRFrame = -1; 
        private bool isLogicUpdate = false;
        private bool isStart = false;
        private bool isRollingBack = false;

        private Thread UpdateThread;
        public bool IsFighting
        {
            get => isStart;
        }      
        public bool IsRollingBack
        {
            get => isRollingBack;
        }
        
        public RebornCore()
        {
            logicFrameDeltaTime = 10000000 / _TargetFrame;
            _TargetDeltaTime = new RebornFloat(logicFrameDeltaTime * RebornMath.PRECISION / 10000000L);
            AppInstance.Instance.AppQuitActions += StopLogicFrame;
        }
        
        public RebornFloat DeltaTime => new RebornFloat(_deltaTimeTick*RebornMath.PRECISION/10000000L);
        
        public int TargetFrame => _TargetFrame;

        public int LogicFrame
        {
            get => logicFrame;
        }

        public void QuickRunToFrame(int frame)
        {
            if(frame <= logicFrame)
                return;
            QRFrame = frame;
        }

        private RebornFloat _TargetDeltaTime;
        public RebornFloat TargetDeltaTime => _TargetDeltaTime;

        public RebornFloat TimeSinceLogicStart => logicFrame.RebornFloat() * TargetDeltaTime;

        
        
        public RebornFloat FramesTransToSeconds(int frames)
        {
            return frames.RebornFloat()/_TargetFrame.RebornFloat();
        }
        public int SecondsTransToFrames(RebornFloat secs)
        {
            return RebornMath.FloorToInt(_TargetFrame.RebornFloat() * secs);
        }

        public void ThreadMethod()
        {
            while (true)
            { 
                var tick = DateTime.Now.Ticks;
                _deltaTimeTick += tick - lastUpdateTick;
                lastUpdateTick = tick;
                Update();
                if (logicFrameDeltaTime - _deltaTimeTick > 2)
                {
                    Thread.Sleep(1);
                }
            }
        }
        public void StopLogicFrame()
        {
            UpdateThread?.Abort();
            UpdateThread = null;
            isLogicUpdate = false;
            ResetLogicFrame();
            isStart = false;
#if LOGIC_LOG
            Debug.Log("逻辑帧结束");
#endif
        }
        
        public void StartLogicFrame()
        {
            if(UpdateThread != null)
                return;
            
            lastUpdateTick = DateTime.Now.Ticks;
            UpdateThread = new Thread(ThreadMethod);
            UpdateThread.Start();
            isStart = true;
            isLogicUpdate = true;
#if LOGIC_LOG
            Debug.Log("逻辑帧开始");
#endif
        }

        public void ResumeLogicFrame()
        {
            lastUpdateTick = DateTime.Now.Ticks;
            isLogicUpdate = true;
            _deltaTimeTick = 0;
#if LOGIC_LOG
            Debug.Log("逻辑帧暂停");
#endif
        }
        public void PauseLogicFrame()
        {
            isLogicUpdate = false;
            _deltaTimeTick = 0;
#if LOGIC_LOG
            Debug.Log("逻辑帧暂停");
#endif
        }
        
        public void ResetLogicFrame()
        {
            logicFrame = -1;
            lastUpdateTick = DateTime.Now.Ticks;
            _deltaTimeTick = 0;
        }

        public void Update()
        {
            OnUpdate?.Invoke();
            
            
            if (isLogicUpdate)
            {
                //运算数据帧
                while (_deltaTimeTick >= logicFrameDeltaTime || logicFrame < QRFrame)
                {
                    var tempTick = DateTime.Now.Ticks;
                    if(isStart)
                        logicFrame++;
                    else
                        QRFrame = logicFrame;
                    
                    
                    OnFixedUpdate?.Invoke(logicFrame);


                    if(QRFrame == -1)
                        _deltaTimeTick -= logicFrameDeltaTime;
                    else if(logicFrame >= QRFrame)
                    {
                        _deltaTimeTick = 0;
                        QRFrame = -1;
                    }
                    if(!isLogicUpdate)
                    {
                        _deltaTimeTick = 0;
                        break;
                    }

                    _CaculateLogicTick = DateTime.Now.Ticks - tempTick;
                }
            }
        }

        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="frame"></param>
        public void RollBack(int frame)
        {
            isRollingBack = true;
            long tempTick = DateTime.Now.Ticks;
            //保存当前帧值
            var tempFream = logicFrame;
            
            //逻辑帧回到开始的时候
            logicFrame = frame;
            
            //通知开始回滚
            OnRollBack?.Invoke(frame);
            
            // 快速计算到当前帧
            while (logicFrame <= tempFream)
            {
                OnFixedUpdate?.Invoke(logicFrame);
                logicFrame++;
            }

            logicFrame = tempFream;
            
            _deltaTimeTick = 0;
            isRollingBack = false;
            _CaculateLogicTick = DateTime.Now.Ticks - tempTick;
        }
        
        
        
#if UNITY_EDITOR && !USE_DLL
        public List<FrameDebugInfo> OldDebugs = new List<FrameDebugInfo>();
        public List<FrameDebugInfo> NewDebugs = new List<FrameDebugInfo>();
        
#endif
        public void AddDebugInfo(string str)
        {
#if UNITY_EDITOR && !USE_DLL
            if (!isStart)
                return;

            if (LogicFrame < NewDebugs.Count - 1)
            {
                var temp = OldDebugs;
                OldDebugs = NewDebugs;
                NewDebugs = temp;
                NewDebugs.Clear();
            }

            for (int i = NewDebugs.Count - 1; i < LogicFrame; i++)
            {
                FrameDebugInfo info = new FrameDebugInfo();
                info.Frame = NewDebugs.Count;
                NewDebugs.Add(info);
            }

            var frameDebugInfo = NewDebugs[LogicFrame];
            DebugInfo debug = new DebugInfo
            {
                Title = str,
                StackInfo = new System.Diagnostics.StackTrace(true).ToString()
            };
            frameDebugInfo.DebugInfos.Add(debug);
#endif
        }

    }
    
#if UNITY_EDITOR && !USE_DLL   
    public class FrameDebugInfo
    {
        public int Frame = 0;
        public List<DebugInfo> DebugInfos = new List<DebugInfo>();
    }

    public struct DebugInfo
    {
        public string Title;
        public string StackInfo;
    }
#endif
}