#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using Eluant;
using OpenRA.Graphics;
using OpenRA.Mods.Common.AI;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("Player")]
	public class PlayerProperties : ScriptPlayerProperties
	{
		public PlayerProperties(ScriptContext context, Player player)
			: base(context, player)
		{
			player.World.AddFrameEndTask(_ => ai = Player.PlayerActor.TraitsImplementing<HackyAI>()
				.FirstOrDefault(h => h.IsEnabled));
		}

		HackyAI ai;

		[Desc("The player's internal name.")]
		public string InternalName { get { return Player.InternalName; } }

		[Desc("The player's name.")]
		public string Name { get { return Player.PlayerName; } }

		[Desc("The player's color.")]
		public HSLColor Color { get { return Player.Color; } }

		[Desc("The player's race. (DEPRECATED! Use the `Faction` property.)")]
		public string Race
		{
			get
			{
				Game.Debug("The property `PlayerProperties.Race` is deprecated! Use `PlayerProperties.Faction` instead!");
				return Player.PlayerReference.Faction;
			}
		}

		[Desc("The player's faction.")]
		public string Faction { get { return Player.PlayerReference.Faction; } }

		[Desc("The player's spawnpoint ID.")]
		public int Spawn { get { return Player.SpawnPoint; } }

		[Desc("The player's team ID.")]
		public int Team { get { return Player.PlayerReference.Team; } }

		[Desc("Returns true if the player is a bot.")]
		public bool IsBot { get { return Player.IsBot; } }

		[Desc("Returns true if the player is non combatant.")]
		public bool IsNonCombatant { get { return Player.NonCombatant; } }

		[Desc("Returns true if the player is the local player.")]
		public bool IsLocalPlayer { get { return Player == (Player.World.RenderPlayer ?? Player.World.LocalPlayer); } }

		[Desc("Returns all living actors staying inside the world for this player.")]
		public Actor[] GetActors()
		{
			return Player.World.Actors.Where(actor => actor.Owner == Player && !actor.IsDead && actor.IsInWorld).ToArray();
		}

		[Desc("Returns an array of actors representing all ground attack units of this player.")]
		public Actor[] GetGroundAttackers()
		{
			return Player.World.ActorsHavingTrait<AttackBase>()
				.Where(a => a.Owner == Player && !a.IsDead && a.IsInWorld && a.Info.HasTraitInfo<MobileInfo>())
				.ToArray();
		}

		[Desc("Returns all living actors of the specified type of this player.")]
		public Actor[] GetActorsByType(string type)
		{
			var result = new List<Actor>();

			ActorInfo ai;
			if (!Context.World.Map.Rules.Actors.TryGetValue(type, out ai))
				throw new LuaException("Unknown actor type '{0}'".F(type));

			result.AddRange(Player.World.Actors
				.Where(actor => actor.Owner == Player && !actor.IsDead && actor.IsInWorld && actor.Info.Name == ai.Name));

			return result.ToArray();
		}

		[Desc("Check if the player has these prerequisites available.")]
		public bool HasPrerequisites(string[] type)
		{
			var tt = Player.PlayerActor.TraitOrDefault<TechTree>();
			if (tt == null)
				throw new LuaException("Missing TechTree trait on player {0}!".F(Player));

			return tt.HasPrerequisites(type);
		}

		public bool QueueProductionItem(string buildingType, CPos location)
		{
			if (!IsBot || ai == null || !ai.IsEnabled)
				return false;

			ActorInfo actorInfo;
			if (Player.World.Map.Rules.Actors.TryGetValue(buildingType, out actorInfo))
				if (ai.QueueProductionItem(actorInfo, location))
					return true;

			return false;
		}

		public CPos GetRandomBaseCenter()
		{
			if (!IsBot || ai == null || !ai.IsEnabled)
				return CPos.Zero;

			return ai.GetRandomBaseCenter();
		}
	}
}
