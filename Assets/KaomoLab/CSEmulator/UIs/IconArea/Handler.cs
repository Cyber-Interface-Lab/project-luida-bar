using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.KaomoLab.CSEmulator.UIs.IconArea
{
    [DisallowMultipleComponent]
    public class Handler
        : MonoBehaviour
    {
        public event Action<int, bool> OnButton = delegate { };

        [SerializeField] List<GameObject> areas;
        [SerializeField] List<RawImage> icons;

        public void Show(int index, Texture2D tex)
        {
            areas[index].SetActive(true);
            icons[index].texture = tex;
        }
        public void Hide(int index)
        {
            areas[index].SetActive(false);
        }
        public void HideAll()
        {
            foreach (var area in areas)
            {
                area.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) OnButton.Invoke(0, true);
            if (Input.GetMouseButtonUp(0))   OnButton.Invoke(0, false);
            if (Input.GetMouseButtonDown(1)) OnButton.Invoke(1, true);
            if (Input.GetMouseButtonUp(1))   OnButton.Invoke(1, false);
            if (Input.GetKeyDown(KeyCode.E)) OnButton.Invoke(2, true);
            if (Input.GetKeyUp(KeyCode.E))   OnButton.Invoke(2, false);
            if (Input.GetKeyDown(KeyCode.R)) OnButton.Invoke(3, true);
            if (Input.GetKeyUp(KeyCode.R))   OnButton.Invoke(3, false);
        }
    }
}
