    using UnityEngine;

    public class PlayerLevelUpState : IPlayerState
    {
        private PlayerController _player;

        public PlayerLevelUpState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            Debug.Log("LEVEL UP STATE: Entered");


            Time.timeScale = 0f;

            _player.levelUpScreen.gameObject.SetActive(true);
            _player.levelUpScreen.Initialize(_player);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Exit()
        {

            Time.timeScale = 1f;

            _player.levelUpScreen.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void HandleInput() {  }
        public void Update() { }
        public void FixedUpdate() { }
    }