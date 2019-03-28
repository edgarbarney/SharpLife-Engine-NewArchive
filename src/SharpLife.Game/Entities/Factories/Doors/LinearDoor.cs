/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.Physics;
using SharpLife.Game.Entities.Components;
using System.Diagnostics;
using System.Numerics;

namespace SharpLife.Game.Entities.Factories.Doors
{
    /// <summary>
    /// A door that uses a <see cref="LinearMovementLocomotor"/> to move
    /// </summary>
    public sealed class LinearDoor : Component
    {
        private LinearMovementLocomotor _locomotor;

        private ToggleState _toggleState = ToggleState.AtBottom;

        private Collider Activator;

        public Vector3 Position1;
        public Vector3 Position2;

        [KeyValue(Name = "wait")]
        public float Wait;

        [KeyValue(Name = "lip")]
        public float Lip;

        //TODO: initialize from spawnflags
        public bool Silent;
        public bool UseOnly;
        public bool NoAutoReturn;
        public bool Passable;
        public bool StartsOpen;

        /// <summary>
        /// If this button has a master switch, this is the targetname.
        /// A master switch must be of the multisource type.
        /// If all of the switches in the multisource have been triggered,
        /// then the button will be allowed to operate.
        /// Otherwise, it will be deactivated.
        /// </summary>
        public string m_sMaster;

        public void Initialize()
        {
            _locomotor = Entity.GetRequiredComponent<LinearMovementLocomotor>();
        }

        public void Touch(Collider other)
        {
            //Only allow touch input if not use only and if not moving
            if (!UseOnly && (_toggleState == ToggleState.AtBottom || _toggleState == ToggleState.AtTop))
            {
                // Ignore touches by anything but players
                if (!other.Entity.IsPlayer())
                {
                    return;
                }

                // If door has master, and it's not ready to trigger, 
                // play 'locked' sound

                if (m_sMaster != null && !EntityUtils.IsMasterTriggered(m_sMaster, other.Entity))
                {
                    //TODO
                    //PlayLockSounds(pev, &m_ls, TRUE, FALSE);
                }

                // If door is somebody's target, then touching does nothing.
                // You have to activate the owner (e.g. button).

                if (!string.IsNullOrEmpty(Entity.TargetName))
                {
                    // play locked sound
                    //TODO
                    //PlayLockSounds(pev, &m_ls, TRUE, FALSE);
                    return;
                }

                Activator = other;// remember who activated the door

                DoorActivate();
            }
        }

        /// <summary>
        /// Causes the door to "do its thing", i.e. start moving, and cascade activation.
        /// </summary>
        public bool DoorActivate()
        {
            var activator = Activator;

            if (!EntityUtils.IsMasterTriggered(m_sMaster, activator?.Entity))
            {
                return false;
            }

            if (NoAutoReturn && _toggleState == ToggleState.AtTop)
            {
                // door should close
                DoorGoDown();
            }
            else
            {
                // door should open
                if (activator?.Entity.IsPlayer() == true)
                {
                    // give health if player opened the door (medikit)
                    // VARS( m_eoActivator )->health += m_bHealthValue;

                    //TODO
                    //activator.TakeHealth(m_bHealthValue, DMG_GENERIC);
                }

                // play door unlock sounds
                //TODO:
                //PlayLockSounds(pev, &m_ls, FALSE, FALSE);

                DoorGoUp();
            }

            return true;
        }

        /// <summary>
        /// Helper to trigger door from object editor
        /// </summary>
        public void DoorTrigger()
        {
            DoorActivate();
        }

        /// <summary>
        /// Starts the door going to its "down" position (simply ToggleData->vecPosition1).
        /// </summary>
        public void DoorGoDown()
        {
            //TODO: need to handle this differently
            if (!Silent)
            {
                if (_toggleState != ToggleState.GoingUp && _toggleState != ToggleState.GoingDown)
                {
                    //TODO
                    //EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving), 1, ATTN_NORM);
                }
            }

#if DOOR_ASSERT
            Debug.Assert(_toggleState == ToggleState.AtTop);
#endif // DOOR_ASSERT

            _toggleState = ToggleState.GoingDown;

            _locomotor.MoveTo(Position1, _locomotor.Transform.Speed, DoorHitBottom);
        }

        /// <summary>
        /// The door has reached the "down" position.  Back to quiescence.
        /// </summary>
        public void DoorHitBottom()
        {
            if (!Silent)
            {
                //TODO
                //STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
                //EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseArrived), 1, ATTN_NORM);
            }

            Debug.Assert(_toggleState == ToggleState.GoingDown);
            _toggleState = ToggleState.AtBottom;

            //TODO
            //SUB_UseTargets(m_hActivator, USE_TOGGLE, 0); // this isn't finished

            // Fire the close target (if startopen is set, then "top" is closed) - netname is the close target
            //TODO
            //if (pev->netname && (SpawnFlags & SF.DoorStartsOpen) == 0)
            {
                //TODO
                //FireTargets(STRING(pev->netname), m_hActivator, this, USE_TOGGLE, 0);
            }
        }

        /// <summary>
        /// Starts the door going to its "up" position (simply ToggleData->vecPosition2).
        /// </summary>
        private void DoorGoUp()
        {
            // It could be going-down, if blocked.
            Debug.Assert(_toggleState == ToggleState.AtBottom || _toggleState == ToggleState.GoingDown);

            // emit door moving and stop sounds on CHAN_STATIC so that the multicast doesn't
            // filter them out and leave a client stuck with looping door sounds!
            if (!Silent)
            {
                if (_toggleState != ToggleState.GoingUp && _toggleState != ToggleState.GoingDown)
                {
                    //TODO
                    //EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving), 1, ATTN_NORM);
                }
            }

            _toggleState = ToggleState.GoingUp;

            _locomotor.MoveTo(Position2, _locomotor.Transform.Speed, DoorHitTop);
        }

        /// <summary>
        /// The door has reached the "up" position.  Either go back down, or wait for another activation.
        /// </summary>
        public void DoorHitTop()
        {
            if (!Silent)
            {
                //TODO
                //STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
                //EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseArrived), 1, ATTN_NORM);
            }

            Debug.Assert(_toggleState == ToggleState.GoingUp);
            _toggleState = ToggleState.AtTop;

            // toggle-doors don't come down automatically, they wait for refire.
            if (!NoAutoReturn)
            {
                // In flWait seconds, DoorGoDown will fire, unless wait is -1, then door stays open
                if (Wait != -1)
                {
                    Invoke(nameof(DoorGoDown), Wait);
                }
            }

            // Fire the close target (if startopen is set, then "top" is closed) - netname is the close target
            //TODO
            //if (pev->netname && (SpawnFlags & SF.DoorStartsOpen) != 0)
            {
                //FireTargets(STRING(pev->netname), m_hActivator, this, USE_TOGGLE, 0);
            }

            //SUB_UseTargets(m_hActivator, USE_TOGGLE, 0); // this isn't finished
        }
    }
}
