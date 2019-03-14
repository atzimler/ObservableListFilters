using System;

namespace ATZ.ObservableListFilters
{
    internal class InternalChange
    {
        private bool _executing;

        public void Execute(Action action)
        {
            if (_executing)
            {
                return;
            }

            try
            {
                _executing = true;

                action();
            }
            finally
            {
                _executing = false;
            }
        }
    }
}