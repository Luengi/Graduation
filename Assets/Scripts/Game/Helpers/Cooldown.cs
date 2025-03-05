using System;
using UnityEngine;

public class Cooldown
{
    private bool _isOnCooldown;
	private float _cooldownTime;
	private float _cooldownTimer;

	public bool IsOnCooldown => _isOnCooldown;

	public event Action OnCooldownOver;

	public void StartCooldown (float cooldownTime)
	{
		if (_isOnCooldown) 
		{
			Debug.LogWarning("Cooldown is already active");
			return;
		}

		_cooldownTime = cooldownTime;
		_cooldownTimer = 0;
		_isOnCooldown = true;
	}

	internal void ResetCooldown()
	{
		_isOnCooldown = false;
	}

	public void DecreaseCooldown (float time)
	{
		if (!_isOnCooldown) return;
	
		_cooldownTimer += time;

		if (IsCooldownOver()) 
		{
			ResetCooldown();
			OnCooldownOver?.Invoke();
		}
	}

	public void StopCooldown()
	{
		ResetCooldown();
	}

	private bool IsCooldownOver() => _cooldownTimer >= _cooldownTime;
}
