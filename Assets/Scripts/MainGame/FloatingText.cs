using System;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

namespace MainGame
{
    public class FloatingText : NetworkBehaviour
    {
        public event Action<FloatingText> finished;
        public float time = 1.5f;
        private Transform m_MainCamera;
        private TextMesh m_TextMesh;

        private void Awake()
        {
            m_TextMesh = GetComponent<TextMesh>();
            m_MainCamera = Camera.main.transform;
        }

        public void Animate()
        {
            transform.DOMove(transform.position + Vector3.up, time).OnKill(() => finished?.Invoke(this));
            //Tween t = transform.DOPath(new Vector3[3], 3, PathType.CubicBezier);
            //t.SetEase(Ease.Linear).SetLoops(2).OnKill(() => finished?.Invoke(this));
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + m_MainCamera.forward);
        }

        public void Set(string value, Color color)
        {
            m_TextMesh.text = value;
            m_TextMesh.color = color;
        }
    }
}