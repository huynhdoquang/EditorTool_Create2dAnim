using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Line
{
    public class SimpleController : MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        [SerializeField] private Button btnChangeTrigger;

        private List<int> listCurTrigger;

        private void Start()
        {
            if (listCurTrigger == null)
                listCurTrigger = new List<int>();
            foreach (var animator in animators)
            {
                listCurTrigger.Add(0);
            }
            ChangeTrigger();

            if (btnChangeTrigger != null)
            {
                btnChangeTrigger.onClick.RemoveListener(ChangeTrigger);
                btnChangeTrigger.onClick.AddListener(ChangeTrigger);
            }
        }
        void ChangeTrigger()
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (listCurTrigger[i] >= animators[i].parameterCount) listCurTrigger[i] = 0;
                var triggerName = animators[i].parameters[listCurTrigger[i]].name;
                animators[i].SetTrigger(triggerName);
                listCurTrigger[i]++;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                ChangeTrigger();
        }
    }
}

