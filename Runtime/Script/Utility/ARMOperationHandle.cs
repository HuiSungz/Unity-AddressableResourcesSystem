
#if ARM_UNITASK
using System;
using Cysharp.Threading.Tasks;

namespace AddressableManage
{
    public class ARMOperationHandle<T>
    {
        #region Fields

        private Action<ARMOperationHandle<T>> _onCompleted;
        private Exception _operationException;
        private bool _isDone;
        private T _result;
        private float _progress;

        public bool IsDone => _isDone;
        public bool HasError => _operationException != null;
        public Exception OperationException => _operationException;
        public T Result => _result;
        
        public float Progress 
        { 
            get => _progress;
            internal set
            {
                if (!(Math.Abs(_progress - value) > 0.01f))
                {
                    return;
                }
                
                _progress = value;
                OnProgressChanged?.Invoke(_progress);
            }
        }
        
        public event Action<ARMOperationHandle<T>> OnCompleted
        {
            add
            {
                if (_isDone)
                {
                    value?.Invoke(this);
                }
                else
                {
                    _onCompleted += value;
                }
            }
            remove => _onCompleted -= value;
        }
        
        public event Action<float> OnProgressChanged;

        #endregion
        
        internal ARMOperationHandle()
        {
            _progress = 0f;
        }
        
        internal void Complete(T result)
        {
            _result = result;
            _isDone = true;
            Progress = 1.0f;
            _onCompleted?.Invoke(this);
            _onCompleted = null;
        }
        
        internal void Fail(Exception exception)
        {
            _operationException = exception;
            _isDone = true;
            _onCompleted?.Invoke(this);
            _onCompleted = null;
        }

        public UniTask<T> AsUniTask()
        {
            if (IsDone)
            {
                return HasError 
                    ? UniTask.FromException<T>(OperationException) 
                    : UniTask.FromResult(Result);
            }

            var uTcs = new UniTaskCompletionSource<T>();
            OnCompleted += handle =>
            {
                if (handle.HasError)
                {
                    uTcs.TrySetException(handle.OperationException);
                }
                else
                {
                    uTcs.TrySetResult(handle.Result);
                }
            };

            return uTcs.Task;
        }
    }
}
#endif