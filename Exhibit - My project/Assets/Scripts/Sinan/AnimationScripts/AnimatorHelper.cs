using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorHelper : MonoBehaviour
{
    [System.Serializable]
    public class AnimationHelperState
    {
        public string animationName;

        [System.NonSerialized]
        public int animationNameHash = -1;

        public int GetAnimNameHash()
        {
            if (animationNameHash == -1)
            {
                animationNameHash = Animator.StringToHash(animationName);
            }

            return animationNameHash;
        }

        public override string ToString()
        {
            return animationName;
        }
    }
    
    [System.Serializable]
    private class LayerData
    {
        public AnimationHelperState activeAnimationState;
        public System.Action animationCompleteCallback = null;
        public float currentlClipTime = 0f;
        public float startTime = 0f;
        public AnimationHelperState nextAnimationState;
        public bool animationFinished = false;
        public bool animationStarted = false;


        public LayerData(AnimationHelperState state, System.Action callback, float startTime)
        {
            this.nextAnimationState = state;
            this.animationCompleteCallback = callback;
            this.startTime = startTime;
            this.animationFinished = false;
            this.animationStarted = false;
        }
        
        public void SetupNewActiveState(AnimationHelperState newActive)
        {
            this.currentlClipTime = this.startTime;
            this.activeAnimationState = newActive;
            this.nextAnimationState = null;
            this.animationFinished = false;
            this.animationStarted = true;
        }
    }

    [SerializeField] private bool m_continueAnimationOnEnable = true;
    [SerializeField] private bool m_turnOnAnimatorOnEnable = true;

    [SerializeField] private Animator m_animator;
    [SerializeField, ReadOnly] private List<AnimationHelperState> m_animStates = new List<AnimationHelperState>();
    private Dictionary<int, LayerData> m_animatorLayerDatas = new Dictionary<int, LayerData>();

    private List<int> m_activeKeys = new List<int>();
    
    private void Awake()
    {
        if (m_animator == null)
        {
            m_animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        // automatically switch the animator on incase it accidentally gets turned off in the editor and then checked in.
        if (m_animator != null)
        {
            m_animator.enabled = m_turnOnAnimatorOnEnable;

            foreach (int layer in m_animatorLayerDatas.Keys)
            {
                AnimationHelperState activeState = m_animatorLayerDatas[layer].activeAnimationState 
                                                   ?? m_animatorLayerDatas[layer].nextAnimationState;
                if (activeState != null && m_continueAnimationOnEnable)
                {
                    m_animator.Play(activeState.animationName, layer, m_animatorLayerDatas[layer].currentlClipTime);

                }
            }
        }
    }

    private void OnDisable()
    {
        if (m_animator != null)
        {
            m_animator.enabled = false;
        }
    }

    public List<AnimationHelperState> GetAnimationList()
    {
        return m_animStates;
    }

    public void SetStates(List<string> allStates)
    {
        m_animStates.Clear();
        if (allStates == null)
        {
            return;
        }
        
        for (int i = 0; i < allStates.Count; i++)
        {
            AnimationHelperState state = new AnimationHelperState();
            state.animationName = allStates[i];
            state.GetAnimNameHash();
            
            m_animStates.Add(state);
        }
    }

    public IList<AnimationHelperState> AnimStates => m_animStates;

    public void SetAnimator(Animator animator)
    {
        m_animator = animator;
    }

    public Animator GetAnimator()
    {
        return m_animator;
    }
    
    public void ChangeAnimaState(int index, string animationName)
    {
        AnimationHelperState animState = m_animStates[index];
        animState.animationName = animationName;
        animState.animationNameHash = -1;
    }
    
    private void PlayAnimationInternal(AnimationHelperState state, Action onFinished = null, float startTime = 0, float crossfade = 0, int layer = 0)
    {
        if (state == null)
        {
            if (onFinished != null)
            {
                onFinished();
            }

            return;
        }

        LayerData newData = new LayerData(state, onFinished, startTime);
        newData.currentlClipTime = 0;
        
        Animator anim = GetAnimator();
        AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(layer);
        
        if (m_animatorLayerDatas.ContainsKey(layer))
        {
            m_animatorLayerDatas[layer] = newData;
        }
        else
        {
            m_animatorLayerDatas.Add(layer, newData);
        }

        if (!m_activeKeys.Contains(layer))
        {
            m_activeKeys.Add(layer);
        }
        
        if (crossfade > 0.0f)
        {
            m_animator.CrossFade(state.animationName, crossfade, layer, 0f);
        }
        else
        {
            m_animator.Play(state.animationName, layer, startTime);
        }
    }
    
    private void Update()
    {
        if (ShouldUpdateAnimState())
        {
            UpdateAnimState();
        }
    }

    private bool ShouldUpdateAnimState()
    {
        return m_animator != null;
    }

    public void Play(string animationName, Action onFinished = null, float startTime = 0f, bool checkIsPlayingFirst = false, float crossFade = 0f, int layer = 0)
    {
        if (!m_animator.enabled)
        {
            // Debugger.Error("Trying to play animation: " + animationName + ", but animator is disabled");
        }
        else
        {
            //Shoud we check if the next animation to play is ALREADY playing?
            if (checkIsPlayingFirst)
            {
                Animator anim = GetAnimator();
                AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(layer);
                bool isCurentAnimPlating = animInfo.IsName(animationName);

                if (isCurentAnimPlating)
                {
                    // Debugger.Error("Trying to play animation: " + animationName + ", but it is alreay playing");
                    return;
                }
            }

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                AnimationHelperState animState = GetAnimStateFromAnimationName(animationName);
                // Debugger.Assert(animState != null, "The animation " + animationName + " trying to be played in (" + gameObject.name + ") doesn't exist");
                if (animState != null)
                {

                    if (m_animator.HasState(layer, Animator.StringToHash(animationName)))
                    {
                        PlayAnimationInternal(animState, onFinished, startTime, crossFade, layer);
                    }
                    else
                    {
                        // Debugger.Error("Trying to play animation: " + animationName + " on GameObject " + gameObject.name + 
                        //                ", but it doesn't exist. Have you edited the controller outside of the object?");
                    }
                }
            }
            else
            {
                // Debugger.Error("Trying to play animation: " + animationName + ", but game object is not activeSelf or InHierachy");
            }
        }
    }

    private void AnimationFinished(int layer)
    {
        if (m_animatorLayerDatas.ContainsKey(layer) && m_activeKeys.Contains(layer))
        {
            m_activeKeys.Remove(layer);

            System.Action action = m_animatorLayerDatas[layer].animationCompleteCallback;
            if (action != null)
            {
                m_animatorLayerDatas[layer].animationCompleteCallback = null;
                action();
            }
        }
    }

    public bool HasAnimationStateWithName(string stateName)
    {
        return GetAnimStateFromAnimationName(stateName) != null;
    }

    private AnimationHelperState GetAnimStateFromAnimationName(string animationName)
    {
        foreach (AnimationHelperState animState in m_animStates)
        {
            if (animState.animationName == animationName)
            {
                return animState;
            }
        }

        return null;
    }

    private AnimationHelperState GetAnimStateFromAnimationNameHash(int animationNameHash)
    {
        foreach (AnimationHelperState animState in m_animStates)
        {
            if (animState.GetAnimNameHash() == animationNameHash)
            {
                return animState;
            }
        }

        return null;
    }

    private void UpdateAnimState()
    {
        int numKeys = m_activeKeys.Count;
        for (int i = 0; i < numKeys; i++)
        {
            if (i >= m_activeKeys.Count)
            {
                continue;
            }

            int layer = m_activeKeys[i];
            AnimatorStateInfo layerStateInfo = m_animator.GetCurrentAnimatorStateInfo(layer);
            m_animatorLayerDatas[layer].currentlClipTime = layerStateInfo.normalizedTime;

            AnimationHelperState nextState = m_animatorLayerDatas[layer].nextAnimationState;

            if (nextState != null)
            {
                if (layerStateInfo.shortNameHash == nextState.GetAnimNameHash())
                {
                    m_animatorLayerDatas[layer].SetupNewActiveState(nextState);
                }
            }
            else
            {
                bool hasFinished = UpdateAnimFinished(layerStateInfo, layer);

                if (!hasFinished)
                {
                    UpdateAnimStarted(layerStateInfo, layer);
                }
            }

        }
    }

    private void UpdateAnimStarted(AnimatorStateInfo stateInfo, int layer)
    {
        int animStateShortNameHash = stateInfo.shortNameHash;

        AnimationHelperState activeStateOnLayer = m_animatorLayerDatas[layer].activeAnimationState;
        int currentAnimStateShortNameHash = activeStateOnLayer != null ? activeStateOnLayer.GetAnimNameHash() : -1;

        if (animStateShortNameHash != currentAnimStateShortNameHash)
        {
            AnimationHelperState helperState = GetAnimStateFromAnimationNameHash(animStateShortNameHash);
            if (helperState != null)
            {
                m_animatorLayerDatas[layer].SetupNewActiveState(helperState);
            }
        }
    }

    private bool UpdateAnimFinished(AnimatorStateInfo stateInfo, int layer)
    {
        int animStateShortNameHash = stateInfo.shortNameHash;
        AnimationHelperState activeStateOnLayer = m_animatorLayerDatas[layer].activeAnimationState;
        int currentAnimStateShortNameHash = activeStateOnLayer != null ? activeStateOnLayer.GetAnimNameHash() : -1;

        if (!m_animatorLayerDatas[layer].animationFinished)
        {
            AnimatorClipInfo[] animClipInfos = m_animator.GetCurrentAnimatorClipInfo(layer);
            bool animClipHasFrames = animClipInfos.Length > 0 && animClipInfos[0].clip.length <= Mathf.Epsilon;
            
            if (stateInfo.normalizedTime >= 1f  || animStateShortNameHash != currentAnimStateShortNameHash || animClipHasFrames)
            {
                m_animatorLayerDatas[layer].animationFinished = true;
                AnimationFinished(layer);
                return true;
            }
        }

        return false;
    }

    public AnimatorControllerParameter[] GetAnimatorParameters()
    {
#if UNITY_EDITOR
        if (m_animator == null)
        {
            return null;
        }

        UnityEditor.Animations.AnimatorController editorAnimatorController = m_animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        if (editorAnimatorController != null)
        {
            return editorAnimatorController.parameters;
        }
#endif
        return null;
    }

    //Helper functions
    public string GetCurrentStateName(int layer = 0)
    {
        if (m_animatorLayerDatas.ContainsKey(layer))
        {
            AnimationHelperState activeState = m_animatorLayerDatas[layer].activeAnimationState;
            if (activeState != null)
            {
                return activeState.animationName;
            }
        }
        
        return "";
    }

    public float GetCurrentStateNormalisedTime(int layer = 0)
    {
        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime;
    }

    public void SetAnimatorSpeed(float speed)
    {
        m_animator.speed = speed;
    }

    public void StopAnimator()
    {
        m_animator.enabled = false;
    }

//Params
#region  Paramaters
    public void SetBool(string key, bool value)
    {
        if (m_animator == null)
        {
            return;
        }
        m_animator.SetBool(key, value);
    }
    
    public void SetInt(string key, int value)
    {
        if (m_animator == null)
        {
            return;
        }
        m_animator.SetInteger(key, value);
    }
    
    public void SetFloat(string key, float value)
    {
        if (m_animator == null)
        {
            return;
        }
        m_animator.SetFloat(key, value);
    }
    
    public void SetTrigger(string key)
    {
        if (m_animator == null)
        {
            return;
        }
        m_animator.SetTrigger(key);
    }
#endregion
}
