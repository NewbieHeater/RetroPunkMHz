using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Decorators
{
    public abstract class Decorator : BTNode
    {
        protected BTNode Child;

        protected Decorator(BTNode child, string name = null) : base(name) { Child = child; }

        public override void Reset(BTContext ctx)
        {
            base.Reset(ctx);
            Child.Reset(ctx);
        }
    }

    public sealed class Inverter : Decorator
    {
        public Inverter(BTNode child, string name = null) : base(child, name) { }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            var s = Child.Tick(ctx);
            if (s == NodeStatus.Running) return NodeStatus.Running;
            return s == NodeStatus.Success ? NodeStatus.Failure : NodeStatus.Success;
        }
    }

    public sealed class Succeeder : Decorator
    {
        public Succeeder(BTNode child, string name = null) : base(child, name) { }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            var s = Child.Tick(ctx);
            return s == NodeStatus.Running ? NodeStatus.Running : NodeStatus.Success;
        }
    }

    public sealed class Repeater : Decorator
    {
        private readonly int _times;        // -1이면 무한
        private readonly bool _untilSuccess;
        private readonly bool _untilFailure;
        private int _count;

        public Repeater(BTNode child, int times = -1, bool untilSuccess = false, bool untilFailure = false, string name = null)
            : base(child, name)
        {
            _times = times;
            _untilSuccess = untilSuccess;
            _untilFailure = untilFailure;
        }

        protected override void OnStart(BTContext ctx) { _count = 0; }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            var s = Child.Tick(ctx);
            if (s == NodeStatus.Running) return NodeStatus.Running;

            if (_untilSuccess && s == NodeStatus.Success) return NodeStatus.Success;
            if (_untilFailure && s == NodeStatus.Failure) return NodeStatus.Success; // "조건 달성"으로 반복 종료를 Success로 취급

            _count++;
            if (_times >= 0 && _count >= _times)
                return NodeStatus.Success;

            // 다음 반복을 위해 리셋
            Child.Reset(ctx);
            return NodeStatus.Running;
        }
    }

    public sealed class Cooldown : Decorator
    {
        private readonly float _seconds;
        private float _nextReadyTime;

        // failWhileCooling: true면 쿨다운 중 바로 Failure 반환, false면 Running 반환
        private readonly bool _failWhileCooling;

        public Cooldown(BTNode child, float seconds, bool failWhileCooling = true, string name = null)
            : base(child, name)
        {
            _seconds = seconds;
            _failWhileCooling = failWhileCooling;
            _nextReadyTime = -999f;
        }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            if (Time.time < _nextReadyTime)
                return _failWhileCooling ? NodeStatus.Failure : NodeStatus.Running;

            var s = Child.Tick(ctx);
            if (s == NodeStatus.Success || s == NodeStatus.Failure)
            {
                _nextReadyTime = Time.time + _seconds;
            }
            return s;
        }
    }

    public sealed class Guard : Decorator
    {
        private readonly Func<BTContext, bool> _predicate;

        public Guard(Func<BTContext, bool> predicate, BTNode child, string name = null) : base(child, name)
        {
            _predicate = predicate;
        }

        protected override NodeStatus OnUpdate(BTContext ctx)
        {
            if (!_predicate(ctx)) return NodeStatus.Failure;
            return Child.Tick(ctx);
        }
    }
}
