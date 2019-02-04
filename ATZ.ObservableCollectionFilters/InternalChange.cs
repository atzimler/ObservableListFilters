using System;

namespace ATZ.ObservableCollectionFilters
{
    public class InternalChange
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