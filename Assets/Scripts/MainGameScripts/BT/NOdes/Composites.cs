using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Composites
{
    public abstract class Composite : BTNode
    {
        protected readonly List<BTNode> Children = new List<BTNode>();

        protected Composite(string name = null) : base(name) { }

        public Composite AddChild(BTNode node) { Children.Add(node); return this; }

        public override void Reset(BTContext ctx)
        {
            base.Reset(ctx);
            foreach (var c in Children) c.Reset(ctx);
        }
    }

    public sealed class Sequence : Composite
    {
        private int _index;

        public Sequence(string name = null) : base(name) { }

        protected override void OnStart(BTContext ctx) { _index = 0; }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            while (_index < Children.Count)
            {
                var s = Children[_index].Tick(ctx);
                if (s == NodeStatus.Running) return NodeStatus.Running;
                if (s == NodeStatus.Failure) return NodeStatus.Failure;
                _index++;
            }
            return NodeStatus.Success;
        }

        protected override void OnStop(BTContext ctx)
        {
            if (_index < Children.Count)
                Children[_index].Abort(ctx);
        }
    }

    public sealed class Selector : Composite
    {
        private int _index;

        public Selector(string name = null) : base(name) { }

        protected override void OnStart(BTContext ctx) { _index = 0; }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            while (_index < Children.Count)
            {
                var s = Children[_index].Tick(ctx);
                if (s == NodeStatus.Running) return NodeStatus.Running;
                if (s == NodeStatus.Success) return NodeStatus.Success;
                _index++;
            }
            return NodeStatus.Failure;
        }

        protected override void OnStop(BTContext ctx)
        {
            if (_index < Children.Count)
                Children[_index].Abort(ctx);
        }
    }

    public sealed class Parallel : Composite
    {
        public enum Policy { RequireAll, RequireOne }

        private NodeStatus[] _last;
        private Policy _successPolicy;
        private Policy _failurePolicy;

        public Parallel(Policy successPolicy, Policy failurePolicy, string name = null) : base(name)
        {
            _successPolicy = successPolicy;
            _failurePolicy = failurePolicy;
        }

        protected override void OnStart(BTContext ctx)
        {
            _last = new NodeStatus[Children.Count];
            for (int i = 0; i < _last.Length; i++) _last[i] = NodeStatus.Running;
        }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                // 완료된 노드는 다시 틱하지 않음
                if (_last[i] == NodeStatus.Running)
                    _last[i] = Children[i].Tick(ctx);

                if (_last[i] == NodeStatus.Success) successCount++;
                else if (_last[i] == NodeStatus.Failure) failureCount++;
            }

            bool success =
                (_successPolicy == Policy.RequireAll && successCount == Children.Count) ||
                (_successPolicy == Policy.RequireOne && successCount > 0);

            bool failure =
                (_failurePolicy == Policy.RequireAll && failureCount == Children.Count) ||
                (_failurePolicy == Policy.RequireOne && failureCount > 0);

            if (success) return NodeStatus.Success;
            if (failure) return NodeStatus.Failure;
            return NodeStatus.Running;
        }

        protected override void OnStop(BTContext ctx)
        {
            foreach (var c in Children) c.Abort(ctx);
        }
    }
}