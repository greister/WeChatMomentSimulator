using System;
using System.Threading;

namespace WeChatMomentSimulator.Desktop.Utils
{
    /// <summary>
    /// 防抖动工具类，用于限制高频事件
    /// </summary>
    public class Debouncer
    {
        private int _delay;
        private Action _action;
        private Timer _timer;
        private bool _isDisposed;
        private readonly object _lock = new object();

        /// <summary>
        /// 使用指定延迟执行操作
        /// </summary>
        /// <param name="milliseconds">延迟毫秒数</param>
        /// <param name="action">要执行的操作</param>
        public void Debounce(int milliseconds, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            _delay = milliseconds;
            _action = action;
            
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Change(milliseconds, Timeout.Infinite);
                }
                else
                {
                    _timer = new Timer(Execute, null, milliseconds, Timeout.Infinite);
                }
            }
        }

        private void Execute(object state)
        {
            lock (_lock)
            {
                if (_isDisposed) return;
                
                // 重置计时器，避免重用
                _timer.Dispose();
                _timer = null;
                
                try
                {
                    // 在UI线程执行操作
                    System.Windows.Application.Current.Dispatcher.Invoke(_action);
                }
                catch
                {
                    // 忽略UI线程调度异常
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed) return;
                
                _isDisposed = true;
                _timer?.Dispose();
                _timer = null;
            }
        }
    }
}