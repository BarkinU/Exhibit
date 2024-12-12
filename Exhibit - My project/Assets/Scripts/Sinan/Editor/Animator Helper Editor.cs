using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Animations;

[CustomEditor(typeof(AnimatorHelper), true)]
public class AnimatorHelperEditor : Editor
{
    private const string CONST_PREFIX = "private const string ";
    private const string ANIM_PREFIX = "ANIM_";
    private const string PARAM_PREFIX = "PARAM_";
    private const int ANIM_LIST_MAX_LINES = 8;

    string m_generatedAnimationCode;
    string m_generatedParametersCode;
    private Vector2 m_animCodeScroll;
    private Vector2 m_paramCodeScroll;
    
    private AnimatorHelper m_animatorHelper;
    private SerializedProperty m_animStates;
   
    void OnEnable()
    {
        m_animatorHelper = (AnimatorHelper)target;
        m_animStates = serializedObject.FindProperty("m_animStates");
    }
   
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UpdateGeneratedUiElements();
        
        if (CheckIfAnimStatesNeedsTobeUpdated())
        {
            EditorGUILayout.HelpBox("Animator States needs update", MessageType.Warning);
            if (GUILayout.Button("Update States"))
            {
                UpdateStates();
            }
        }
		
        //Show all the animation names in code format
        if (!string.IsNullOrEmpty(m_generatedAnimationCode))
        {
            EditorGUILayout.Space();
            m_animCodeScroll = EditorGUILayout.BeginScrollView(m_animCodeScroll, GUIStyle.none, GUI.skin.verticalScrollbar, 
                GUILayout.Height( EditorGUIUtility.singleLineHeight * ANIM_LIST_MAX_LINES));
            EditorGUILayout.TextArea(m_generatedAnimationCode);
            EditorGUILayout.EndScrollView();
        }
		
        if (!string.IsNullOrEmpty(m_generatedParametersCode))
        {
            EditorGUILayout.Space();
            m_paramCodeScroll = EditorGUILayout.BeginScrollView(m_paramCodeScroll, GUIStyle.none, GUI.skin.verticalScrollbar, 
                GUILayout.Height( EditorGUIUtility.singleLineHeight * ANIM_LIST_MAX_LINES));
            EditorGUILayout.TextArea(m_generatedParametersCode, GUILayout.Height( EditorGUIUtility.singleLineHeight * ANIM_LIST_MAX_LINES));
            EditorGUILayout.EndScrollView();
        }
        
    }

    private void UpdateStates()
    {
        List<string> animatorStates = GetStateNamesInAnimator();
        m_animatorHelper.SetStates(animatorStates);
        EditorUtility.SetDirty(target);
        serializedObject.Update();
    }

    private void UpdateGeneratedUiElements()
    {
        ReloadPreviewInstances();
        GenerateStateNamesCode();
        GenerateParameterNameCode();
    }

    private void GenerateStateNamesCode()
    {
        List<AnimatorHelper.AnimationHelperState> animationsList = m_animatorHelper.GetAnimationList();
        m_generatedAnimationCode = "";
        for (int i = 0; i < animationsList.Count; ++i)
        {
            string newLine = i < (animationsList.Count - 1) ? "\n" : String.Empty;
            string animation = animationsList[i].animationName;
            string constName = FormatNameForConst(animation);
            m_generatedAnimationCode += CONST_PREFIX + ANIM_PREFIX + constName + " = \"" + animation + "\";" + newLine;
        }
    }
    
    private void GenerateParameterNameCode()
    {
        m_generatedParametersCode = "";

        AnimatorControllerParameter[] parameters = m_animatorHelper.GetAnimatorParameters();
        if (parameters == null)
        {
            return;
        }

        int numParameters = parameters.Length;
        if (numParameters > 0)
        {
            for (int i = 0; i < numParameters; i++)
            {
                string typeString = "";
                if (parameters[i].type == AnimatorControllerParameterType.Bool)
                {
                    typeString = "_B";
                }
                else if (parameters[i].type == AnimatorControllerParameterType.Trigger)
                {
                    typeString = "_T";
                }
                else if (parameters[i].type == AnimatorControllerParameterType.Float)
                {
                    typeString = "_F";
                }
                else if (parameters[i].type == AnimatorControllerParameterType.Int)
                {
                    typeString = "_I";
                }
                string newLine = i < (numParameters- 1) ? "\n" : String.Empty;

                string paramName = parameters[i].name;
                string constName = FormatNameForConst(paramName);
                string generatedString = CONST_PREFIX + PARAM_PREFIX  + constName + typeString + " = \"" + paramName + "\";" + newLine;
                m_generatedParametersCode += generatedString;
            }
        }
    }

    private string FormatNameForConst(string originalName)
    {
        string constName = originalName;
        constName = constName.ToUpper();
        constName = constName.Replace(" ", "_");
        constName = constName.Replace('-', '_');
        return constName;
    }

    private List<string> GetStateNamesInAnimator()
    {
        AnimatorController ac = m_animatorHelper.GetAnimator().runtimeAnimatorController as AnimatorController;
        List<string> stateNames = new List<string>();
        AnimatorControllerLayer[] layers = ac.layers;
        PropertyInfo statesRecursive = typeof(AnimatorStateMachine).GetProperty("statesRecursive",BindingFlags.Instance | BindingFlags.GetProperty |
            BindingFlags.NonPublic);
        foreach (AnimatorControllerLayer layer in layers)
        {
            AnimatorStateMachine sm = layer.stateMachine;
            List<ChildAnimatorState> states = (List<ChildAnimatorState>)statesRecursive.GetValue(sm);
            for (int i = 0; i < states.Count; ++i)
            {
                stateNames.Add(states[i].state.name);
            }
        }

        return stateNames;
    }

    private bool CheckIfAnimStatesNeedsTobeUpdated()
    {
        AnimatorController ac = m_animatorHelper.GetAnimator().runtimeAnimatorController as AnimatorController;
        List<string> stateNames = GetStateNamesInAnimator();
        IEnumerable<string> animStateNames = m_animatorHelper.AnimStates.Select(x => x.animationName);
        bool sameLists = Enumerable.SequenceEqual(animStateNames.OrderBy(x => x), stateNames.OrderBy(x => x));
        return stateNames.Count <= 0 || !sameLists;
    }
}