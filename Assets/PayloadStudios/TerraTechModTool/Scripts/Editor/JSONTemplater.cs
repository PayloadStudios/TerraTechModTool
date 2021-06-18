using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONTemplater
{
	private bool m_Wheel = false,
		m_Wing = false,
		m_Cab = false,
		m_Gun = false,
		m_Anchor = false,
		m_SkyAnchor = false,
		m_FuelTank = false,
		m_Recipe = false;

	public string GetContents()
	{
		string contents = "{\r\n";

		if(m_Recipe)
		{
			contents +=
@"	""Recipe"": ""plumbiaingot, plubonicgreebles"",
";
		}

		if (m_Wheel)
		{
			contents +=
@"	""Wheel"": 
	{
		// Torque
		""MaxTorque"": 325,
		""PassiveBrakeMaxTorque"": 250,
		""MaxRPM"": 600,
		""ReverseMaxRPM"": 50,
		""BasicFrictionTorque"": 5,
		""CompressedFrictionTorque"": 175,

		""UseTireTracks"": true,
		// WheelTrackType can be ""SmoothWheels"", ""StandardTreads"", ""DeepTreads"" or ""TankTreads""
		""WheelTrackType"": ""StandardTreads"",
		// AudioType can be ""RubberWheel"", ""LargeRubberWheel"", ""SmallWheel"", ""MetalWheel"", ""TankTrack"",
		// ""VentureWheel"" or ""HawkeyeWheel""
		""AudioType"": ""RubberWheel"",

		// Tires
		""FrictionScaleLatitudinal"": 1.0,
		""FrictionScaleLongitudinal"": 1.0,
		""GripFactorLatitudinal"": 3.0,
		""GripFactorLongitudinal"": 2.0,
		""WheelRadius"": 0.45,
		""AngularThickness"": 20.0,
		""SuspensionSpringStrength"": 900,
		""SuspensionDamperStrength"": 130,
		""MaxSuspensionAcceleration"": 0.0,
		""QuadraticSuspension"": true,
		""SuspensionDistance"": 0.25,
		""MaxSteerAngle"": 15,
		""SteerSpeed"": 3,

		""DriveTurnPower"": 0.9,
		""DriveTurnBrake"": 0.1,
		""DriveTurnDifferential"": 0.8,
		""TurnOnSpotPower"": 0.6,
		""MinimumRPMForDust"": 20.0,
	},
";
		}

		if (m_Wing)
		{
			contents +=
@"	""Wing"": 
	{
		""AttackAngleDamping"": 1.0,
		""TrailMinimumVelocity"": 8.0,
		""TrailTransparency"": 0.01,
		""TrailFadeSpeed"": 0.5,
		""Aerofoils"": 
		[
			{
				""ObjectName"": ""_aerofoil_1"",
				""LiftStrength"": 1.0,
				""FlapAngleRangeActual"": 30.0,
				""FlapAngleRangeVisual"": 30.0,
				""FlapTurnSpeed"": 1.0,
				""SpinnerRotationAxis"": ""X"",
				""SpinnerSteerAxis"": ""Y"",
				""SpinnerAutoSpin"": false,
				""SpinnerSpinUpTime"": 1.0,
			}
		],
		""SmokeTrails"":
		[
			{
				""ObjectName"": ""_smokeTrail_1"",
				""NumberOfPoints"": 10,
				""UpdateSpeed"": 0.07,
				""RiseSpeed"": 0.1,
				""Spread"": 0.1,
			}
		]
	},
";
		}

		if(m_Cab)
		{
			contents += 
@"	""Cab"": 
	{
		""AcceptPlayerInput"":true,
		""EnabledAITypes"":
		[
			""Harvest"",
			""Idle""
		],

		""DefaultThrottle"": 1.0,
		""TurnAngleFullThrottle"": 45.0,
		""ThrottleD"": 0.0,
		""ThrottleT"": 0.0,
		""OuterTurnTolerance"": 45.0,
		""InnerTurnTolerance"": 20.0,
		""PoweredTurnInsideWheel"": 0.2,
		""StopCirclingDelay"": 2.0,
		""IdealTargetRange"": 10.0,
		""LostTargetMemoryTime"": 3.0,
		""HoldTargetDuration"": 0.5,
		""WaypointReachedTolerance"": 2.0,
		""WaypointPlayerAngularBias"": 45.0,
		""WaypointDistanceFullThrottle"": 5.0,
		""LookAroundAngleMin"": 10.0,
		""LookAroundAngleMax"": 80.0,
		""LookAroundPauseMin"": 0.25,
		""LookAroundPauseMax"": 0.5,
		""LookAroundThrottle"": 0.5,
		""DefaultPatrolDistanceMin"": 5,
		""DefaultPatrolDistanceMax"": 15,
		""PatrolThrottle"": 0.95,
		""ControlPriority"": 50,
		""RecoverTimeout"": 3.0,
		""ForceUncapsizeTimeout"": 5.0,
		""CapsizedMinSpeed"": 1.5,
	},
";
		}

		if (m_FuelTank)
		{
			contents +=
@"	""FuelTank"": 
	{
		""Capacity"": 40.0,
		""RefillRate"": 1.0,
	},
";
		}

		if(m_Anchor)
		{
			contents +=
@"	""Anchor"":
	{
		""MaxAngularVelocity"": 0.0,
		""MaxTorque"": 30000.0,
		""BrakeTorque"": 20000.0,
		""ForceHorizontal"": true,
		
		""SnapToleranceUp"": 2.0,
		""SnapToleranceDown"": 80.0,


		""IsSkyAnchor"": false,
		""SkyAnchorStrength"": 1.0,
		""SkyAnchorDamping"": 1.0,
		""SkyAnchorSpeed"": 100.0,
		""SkyAnchorRecoilForce"": 2.0,
	},
";
		}

		if(m_Gun)
		{
			contents +=
@"	""Gun"": 
	{		
		// -- Gimbal settings (how the gun rotates) --
		""LimitedShootAngle"": 0.0,
		""RotateSpeed"": 100.0,
		// GimbalBase refers to the yaw gimbal (rotating left and right)
		// If min and max rotation are set to 0, the gimbal has unlimited movement and can loop around
		""GimbalBaseMinRotation"": 0.0,
		""GimbalBaseMaxRotation"": 0.0,
		""GimbalBaseAimClampMaxPercent"": 1.0,
		""GimbalBaseXAngleAimOffset"": 0.0,
		// GimbalElev refers to the pitch gimbal (looking up and down)
		""GimbalElevMinRotation"": -10.0,
		""GimbalElevMaxRotation"": 20.0,
		""GimbalElevAimClampMaxPercent"": 1.0,
		""GimbalElevXAngleAimOffset"": 0.0,

		// -- Firing --
		// Valid options are ""AutoAim"" or ""Default""
		""AimType"": ""AutoAim"",
		""ChangeTargetInterval"": 0.5,
		""AutoFire"": false,
		""PreventShootingTowardsFloor"": false,
		""DeployWhenHasTarget"": false,
		""DontFireIfNotAimingAtTarget"": false,
		""ShotCooldown"": 0.25,
		""CooldownVariance"": 0.05,
		// Valid options are ""Sequenced"" or ""AllAtOnce""
		""FireControlMode"": ""Sequenced"",
		""SeekingRounds"": false,
		""RegisterWarningAfter"": 1.0,
		""ResetFiringTimeAfterNotFiredFor"": 1.0,
		""OverheatTime"": 0.0,
		""OverheatPauseWindow"": 0.0,
		// See Appendix below for list of available options
		""Bullet"": ""GSO_Bullet_MGun"",
		""Casing"": ""Casing_Micro"",
		// Set count to 0 if you don't want any burst-fire effects
		""BurstShotCount"": 0,
		""BurstCooldown"": 0.0,
		""ResetBurstOnInterrupt"": true,

		// -- Bullet settings --
		""MuzzleVelocity"": 25.0,
		""BulletSprayVariance"": 0.1,
		""BulletSpin"": 0,
		""CasingVelocity"": 5.0,
		""CasingEjectVariance"": 0.3,
		""CasingEjectSpin"": 50,
		""KickbackStrength"":5 ,

		// -- Anims and Effects --
		""HasSpinUpDownAnim"": false,
		""HasCooldownAnim"": false,
		""CanInterruptSpinUp"": false,
		""CanInterruptSpinDown"": false,
		""SpinUpAnimLayerIndex"": 0,
		""MuzzleFlashSpeedFactor"": 1.0,
		""ShowParticlesOnAllQualitySettings"": false,

		// -- Sounds --
		// See wiki for full list of sound effects
		""SoundEffectType"": ""LightMachineGun"",
		""DeploySoundEffectType"": ""Default"",
		""DisableMainAudioLoop"": false,
	},
";
		}
		

		contents += "\r\n}";

		return contents;
	}

	public void CreateObjects(GameObject prefab)
	{
		if(m_Wheel)
		{
			GameObject wheelOrigin = AddChild(prefab, "wheelOrigin");
			AddChild(wheelOrigin, "_wheel").AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>();
			AddChild(prefab, "_sparksLocator");
		}
		if(m_Wing)
		{
			AddChild(prefab, "_aerofoil_1");
			AddChild(prefab, "_smokeTrail_1");
		}
		if (m_Anchor)
		{
			GameObject anchor = AddChild(prefab, "_anchor");
			AddChild(anchor, "_geometry");
			AddChild(anchor, "_groundPoint");
			if (m_SkyAnchor)
			{
				AddChild(anchor, "_fire");
				AddChild(anchor, "_beamAttach");
			}
		}
		if (m_Gun)
		{
			
			GameObject gimbalBase = AddChild(prefab, "_gimbalBase");
			AddChild(gimbalBase, "model_mount").AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>();
			GameObject gimbalElev = AddChild(gimbalBase, "_gimbalElev");
			AddChild(gimbalElev, "model_body").AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>();
			GameObject barrel = AddChild(gimbalElev, "_barrel");
			AddChild(barrel, "_bulletSpawn");
			AddChild(barrel, "_casingSpawn");
			GameObject muzzleFlash = AddChild(barrel, "_muzzleFlash");
			GameObject muzzleFlashAnim = AddChild(muzzleFlash, "_muzzleFlashAnim");
			AddChild(muzzleFlashAnim, "m_MuzzleFlash_01");
			AddChild(muzzleFlash, "_light").AddComponent<Light>();
			GameObject recoiler = AddChild(barrel, "_recoiler");
			AddChild(recoiler, "model_barrel").AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>();
			AddChild(barrel, "_smoke");
		}
	}

	private GameObject AddChild(GameObject parent, string name)
	{
		GameObject child = new GameObject(name);
		child.transform.SetParent(parent.transform);
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		child.transform.localScale = Vector3.one;
		return child;
	}

	public void DrawButtons()
	{
		GUILayout.BeginHorizontal();
		m_Wheel = GUILayout.Toggle(m_Wheel, "Wheel");
		m_Gun = GUILayout.Toggle(m_Gun, "Gun");
		m_Anchor = GUILayout.Toggle(m_Anchor, "Anchor");
		if (m_Anchor)
			m_SkyAnchor = GUILayout.Toggle(m_SkyAnchor, "[Sky Anchor]");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		m_Cab = GUILayout.Toggle(m_Cab, "Cab");
		m_FuelTank = GUILayout.Toggle(m_FuelTank, "Fuel Tank");
		m_Recipe = GUILayout.Toggle(m_Recipe, "Fabricator Recipe");
		m_Wing = GUILayout.Toggle(m_Wing, "Wing");
		GUILayout.EndHorizontal();
		
	}
}
