//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Base class for Skip-Object and Take-Object cmdlets.
//
// Creation Date: Nov 21, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands
{
    internal class PartitionObject
    {
        public int Index { get; private set; }
        public PSObject Item { get; private set; }

        public PartitionObject(int index, PSObject item)
        {
            this.Index = index;
            this.Item = item;
        }
    }

    public abstract class PartitionObjectCommandBase : PscxCmdlet
    {
        private Queue<PartitionObject> _lastItemsQueue = new Queue<PartitionObject>();
        private PSObject _inputObject;
        private int _firstNItems;
        private int _lastNItems;
        private int[] _selectedIndices = new int[0];
        private int _curObjIndex;
        private int _curSelectedIndicesIndex;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject
        {
            get { return _inputObject; }
            set { _inputObject = value; }
        }

        [Parameter(Mandatory = false, Position = 0)]
        [DefaultValue(0)]
        [ValidateRange(0, 0x7fffffff)]
        public int First
        {
            get { return _firstNItems; }
            set { _firstNItems = value; }
        }

        [Parameter(Mandatory = false, Position = 1)]
        [DefaultValue(0)]
        [ValidateRange(0, 0x7fffffff)]
        public int Last
        {
            get { return _lastNItems; }
            set { _lastNItems = value; }
        }

        [Parameter(Mandatory = false, Position = 2)]
        public int[] Index
        {
            get { return _selectedIndices; }
            set { _selectedIndices = value; }
        }

        protected override void BeginProcessing()
        {
            Array.Sort(_selectedIndices);
        }

        protected override void ProcessRecord()
        {
            if (_curObjIndex < _firstNItems)
            {
                FirstItemMatchImpl(_inputObject);
            }
            else if (_lastNItems > 0)
            {
                if (_lastItemsQueue.Count == _lastNItems)
                {
                    PartitionObject obj = _lastItemsQueue.Dequeue();
                    if (IsSelectedIndexMatch(obj.Index))
                    {
                        SelectedIndexMatchImpl(obj.Item);
                    }
                    else
                    {
                        NonSelectedItemImpl(obj.Item);
                    }
                }
                _lastItemsQueue.Enqueue(new PartitionObject(_curObjIndex, _inputObject));
            }
            else
            {
                if (IsSelectedIndexMatch(_curObjIndex))
                {
                    SelectedIndexMatchImpl(_inputObject);
                }
                else
                {
                    NonSelectedItemImpl(_inputObject);
                }
            }

            _curObjIndex++;
        }

        protected override void EndProcessing()
        {
            while (_lastItemsQueue.Count > 0)
            {
                PartitionObject obj = _lastItemsQueue.Dequeue();
                LastItemMatchImpl(obj.Item);
            }
        }

        protected virtual void FirstItemMatchImpl(object inputObject)
        {
        }

        protected virtual void SelectedIndexMatchImpl(object inputObject)
        {
        }

        protected virtual void NonSelectedItemImpl(object inputObject)
        {
        }

        protected virtual void LastItemMatchImpl(object inputObject)
        {
        }

        private bool IsSelectedIndexMatch(int index)
        {
            if (_curSelectedIndicesIndex >= _selectedIndices.Length) return false;

            // Move to the next relevant index in the supplied (sorted) array of indices
            while ((_curSelectedIndicesIndex < _selectedIndices.Length) && (_selectedIndices[_curSelectedIndicesIndex] <= index))
            {
                if (index == _selectedIndices[_curSelectedIndicesIndex]) return true;
                _curSelectedIndicesIndex++;
            }

            return false;
        }
    }
}
