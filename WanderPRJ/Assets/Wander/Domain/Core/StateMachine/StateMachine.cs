using System;
using System.Collections.Generic;

namespace Wander.Domain.Core.StateMachine
{
    public class StateMachine 
    {
        private class Transition 
        {
            public IState NewState { get; }
            public Func<bool> SwitchCondition { get; }

            public Transition(IState nextState, Func<bool> condition) 
            {
                NewState = nextState;
                SwitchCondition = condition;
            }
        }

        private IState _currentState;

        private readonly Dictionary<Type, List<Transition>>
            _transitions = new(); //các State và danh sách transition tương ứng

        private List<Transition> _currentTransitions = new(); //danh sách transition của state hiện tại

        private readonly List<Transition>
            _fromAnyTransitions = new(); //các transition được ưu tiên và có thể chuyển từ bất cứ state nào

        private readonly Dictionary<string, IState> _states = new(); //tên State (string) và State tương ứng

        private static readonly List<Transition> EmptyTransitions = new(0); // danh sách Transition rỗng

        public IState GetCurrentState() {
            return _currentState;
        }

        public void SetState(IState state) {
            if (state == null) return; // guard: tránh NRE nếu lỡ gọi SetState(null)

            //không chuyển state nếu là state cũ
            if (_currentState == state) return;

            _currentState?.Exit();
            _currentState = state;

            //lấy danh sách transition cho state mới
            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null) _currentTransitions = EmptyTransitions;

            _currentState?.Enter();
        }

        public void Tick() {
            //Lấy transition hợp lệ
            var trans = GetTransition();
            //Có transition hợp lệ => đổi State
            if (trans != null) SetState(trans.NewState);

            _currentState?.Tick();
        }

        // FixedTick: gọi từ FixedUpdate() nếu state cần logic theo physics timestep
        // (vd: state điều khiển Rigidbody). KHÔNG evaluate transition ở đây — transition
        // chỉ evaluate 1 lần/frame trong Tick() để tránh state đổi 2 lần trong cùng 1 frame
        // khi FixedUpdate chạy nhiều lần giữa 2 lần Update. Game hiện tại (turn-based)
        // không cần gọi FixedTick, thêm sẵn để dùng khi có state cần đồng bộ vật lý sau này.
        public void FixedTick() {
            _currentState?.FixedTick();
        }

        // lấy transition hợp lệ
        private Transition GetTransition() {
            foreach (var transition in _fromAnyTransitions)
                if (transition.SwitchCondition())
                    return transition;

            foreach (var transition in _currentTransitions)
                if (transition.SwitchCondition())
                    return transition;

            return null;
        }

        // Đăng ký một transition mới: từ state 'from' sang state 'to' khi 'condition' thỏa mãn
        public void AddTransition(IState from, IState to, Func<bool> condition) {
            // Lưu cặp tên - state vào dictionary để tra cứu
            _states[from.GetType().Name] = from;
            _states[to.GetType().Name] = to;

            // Nếu state 'from' chưa có danh sách transition, tạo mới
            if (!_transitions.TryGetValue(from.GetType(), out var transitions)) {
                // tạo List mới
                transitions = new List<Transition>();
                //gán danh sách transition của state hiện tại bằng List mới 
                _transitions[from.GetType()] = transitions;
            }

            // thêm Transition vào danh sách chuyển của State from
            transitions.Add(new Transition(to, condition));
        }

        //thêm transition vào danh sách chuyển đặc biệt
        public void AddAnyTransition(IState to, Func<bool> condition) {
            _fromAnyTransitions.Add(new Transition(to, condition));
        }

        //tra cứu State theo tên (string)
        public IState GetStateByName(string name) {
            return _states.GetValueOrDefault(name, null);
        }
    }
}
    