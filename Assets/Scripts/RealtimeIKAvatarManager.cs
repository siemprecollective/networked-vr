using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Serialization;

namespace Normal.Realtime
{
    [RequireComponent(typeof(Realtime))]
    public class RealtimeIKAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649 // Disable variable is never assigned to warning.
        [FormerlySerializedAs("_avatarPrefab")]
        [SerializeField] private GameObject _localAvatarPrefabKumail;
        [SerializeField] private GameObject _localAvatarPrefabCyrus;
        [SerializeField] private GameObject _localAvatarPrefabPhillip;
        private GameObject _localAvatarPrefab;
        [SerializeField] private RealtimeIKAvatar.LocalPlayer _localPlayer;
#pragma warning restore 0649

        public Transform phillipChairParent;
        public Transform cyrusChairParent;
        public Transform kumailChairParent;

        public GameObject localAvatarPrefab
        {
            get { return _localAvatarPrefab; }
            set { SetLocalAvatarPrefab(value); }
        }

        public RealtimeIKAvatar localAvatar { get; private set; }
        public Dictionary<int, RealtimeIKAvatar> avatars { get; private set; }

        public delegate void AvatarCreatedDestroyed(RealtimeIKAvatarManager avatarManager, RealtimeIKAvatar avatar, bool isLocalAvatar);
        public event AvatarCreatedDestroyed avatarCreated;
        public event AvatarCreatedDestroyed avatarDestroyed;

        private Realtime _realtime;

        private WhichPlayer whichPlayer;

        void Awake()
        {
            _realtime = GetComponent<Realtime>();
            _realtime.didConnectToRoom += DidConnectToRoom;

            if (_localPlayer == null)
                _localPlayer = new RealtimeIKAvatar.LocalPlayer();

            avatars = new Dictionary<int, RealtimeIKAvatar>();
        }

        void Start()
        {
            whichPlayer = GetComponent<WhichPlayer>();
            if (whichPlayer == null)
            {
                Debug.LogError("whichPlayer was not found!");
            }
            else
            {
                switch (whichPlayer.localPlayer)
                {
                    case Players.Cyrus:
                        _localAvatarPrefab = _localAvatarPrefabCyrus;
                        break;
                    case Players.Kumail:
                        _localAvatarPrefab = _localAvatarPrefabKumail;
                        break;
                    case Players.Phillip:
                        _localAvatarPrefab = _localAvatarPrefabPhillip;
                        break;
                }
            }
        }

        private void OnEnable()
        {
            // Create avatar if we're already connected
            if (_realtime.connected)
                CreateAvatarIfNeeded();
        }

        private void OnDisable()
        {
            // Destroy avatar if needed
            DestroyAvatarIfNeeded();
        }

        void OnDestroy()
        {
            _realtime.didConnectToRoom -= DidConnectToRoom;
        }

        void DidConnectToRoom(Realtime room)
        {
            if (!gameObject.activeInHierarchy || !enabled)
                return;

            // Create avatar
            CreateAvatarIfNeeded();
        }

        public void _RegisterAvatar(int clientID, RealtimeIKAvatar avatar)
        {
            if (avatars.ContainsKey(clientID))
            {
                Debug.LogError("RealtimeIKAvatar registered more than once for the same clientID (" + clientID + "). This is a bug!");
            }
            avatars[clientID] = avatar;

            // Fire event
            if (avatarCreated != null)
            {
                try
                {
                    avatarCreated(this, avatar, clientID == _realtime.clientID);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public void _UnregisterAvatar(RealtimeIKAvatar avatar)
        {
            bool isLocalAvatar = false;

            List<KeyValuePair<int, RealtimeIKAvatar>> matchingAvatars = avatars.Where(keyValuePair => keyValuePair.Value == avatar).ToList();
            foreach (KeyValuePair<int, RealtimeIKAvatar> matchingAvatar in matchingAvatars)
            {
                int avatarClientID = matchingAvatar.Key;
                avatars.Remove(avatarClientID);

                isLocalAvatar = isLocalAvatar || avatarClientID == _realtime.clientID;
            }

            // Fire event
            if (avatarDestroyed != null)
            {
                try
                {
                    avatarDestroyed(this, avatar, isLocalAvatar);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private void SetLocalAvatarPrefab(GameObject localAvatarPrefab)
        {
            if (localAvatarPrefab == _localAvatarPrefab)
                return;

            _localAvatarPrefab = localAvatarPrefab;

            // Replace the existing avatar if we've already instantiated the old prefab.
            if (localAvatar != null)
            {
                DestroyAvatarIfNeeded();
                CreateAvatarIfNeeded();
            }
        }

        public void CreateAvatarIfNeeded()
        {
            if (!_realtime.connected)
            {
                Debug.LogError("RealtimeIKAvatarManager: Unable to create avatar. Realtime is not connected to a room.");
                return;
            }

            if (localAvatar != null)
                return;

            if (_localAvatarPrefab == null)
            {
                Debug.LogWarning("Realtime Avatars local avatar prefab is null. No avatar prefab will be instantiated for the local player.");
                return;
            }

            GameObject avatarGameObject = Realtime.Instantiate(_localAvatarPrefab.name, true, true, true, _realtime);
            if (avatarGameObject == null)
            {
                Debug.LogError("RealtimeIKAvatarManager: Failed to instantiate RealtimeIKAvatar prefab for the local player.");
                return;
            }

            localAvatar = avatarGameObject.GetComponent<RealtimeIKAvatar>();
            if (avatarGameObject == null)
            {
                Debug.LogError("RealtimeIKAvatarManager: Successfully instantiated avatar prefab, but could not find the RealtimeIKAvatar component.");
                return;
            }

            localAvatar.localPlayer = _localPlayer;
            if (whichPlayer != null)
            {
                switch (whichPlayer.localPlayer)
                {
                    case Players.Phillip:
                        localAvatar.chairParentObject = phillipChairParent; 
                        break;
                    case Players.Cyrus:
                        localAvatar.chairParentObject = cyrusChairParent; 
                        break;
                    case Players.Kumail:
                        localAvatar.chairParentObject = kumailChairParent; 
                        break;
                }
            }
        }

        public void DestroyAvatarIfNeeded()
        {
            if (localAvatar == null)
                return;

            Realtime.Destroy(localAvatar.gameObject);

            localAvatar = null;
        }

        public void Update() {
            if (_localPlayer.oculusBlendshapeMesh == null) {
                var ocMesh = GameObject.Find("/LocalAvatar/body/body_renderPart_0");
                if (ocMesh != null) {
                    Debug.Log("found LocalAvatar mesh");
                    var renderer = ocMesh.GetComponent<SkinnedMeshRenderer>();
                    _localPlayer.oculusBlendshapeMesh = renderer;
                }
            }
        }
    }
}
