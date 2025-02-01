using Unity.Netcode;
using UnityEngine;

namespace Invector.Utils
{
    public class vComment : NetworkBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] protected string header = "COMMENT";
        [Multiline]
        [SerializeField] protected string comment;

        [SerializeField] protected bool inEdit;

#endif
    }
}