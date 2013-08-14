﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeltaEngine.Core;
using DeltaEngine.Extensions;

namespace DeltaEngine.Entities
{
	/// <summary>
	/// Keeps a list of all active entities and manages all behaviors for entities. Updating is done
	/// at a fixed time step (e.g. 0.1). Drawing has read-only access, it runs as quickly as possible
	/// (60fps+). Details: http://gafferongames.com/game-physics/fix-your-timestep/
	/// </summary>
	public class EntitiesRunner : IDisposable
	{
		public EntitiesRunner(BehaviorResolver behaviorResolver, Settings settings)
		{
			if (settings.UpdatesPerSecond < 1 || settings.RapidUpdatesPerSecond < 1)
				throw new InvalidUpdatePerSecondSettings();
			Current = this;
			this.behaviorResolver = behaviorResolver;
			Time.Delta = UpdateTimeStep = Time.SpeedFactor / settings.UpdatesPerSecond;
			RapidUpdateTimeStep = 1.0f / settings.RapidUpdatesPerSecond;
			for (int priority = 0; priority < prioritizedEntities.Length; priority++)
				prioritizedEntities[priority] = new PrioritizedEntities();
		}

		public class InvalidUpdatePerSecondSettings : Exception {}

		public static EntitiesRunner Current { get; private set; }
		private readonly BehaviorResolver behaviorResolver;
		protected float UpdateTimeStep { get; set; }
		private float RapidUpdateTimeStep { get; set; }
		private readonly PrioritizedEntities[] prioritizedEntities =
			new PrioritizedEntities[Priority.First.GetCount()];

		/// <summary>
		/// Changes how many times UpdateBehaviors and Updateables are called each second. By default
		/// this setting is initialized at the beginning from Settings (20 UpdatesPerSecond = 0.05).
		/// </summary>
		public void ChangeUpdateTimeStep(float newUpdateTimeStep)
		{
			if (newUpdateTimeStep <= 0)
				throw new InvalidUpdatePerSecondSettings();
			Time.Delta = UpdateTimeStep = newUpdateTimeStep;
		}

		public void Dispose()
		{
			Current = null;
		}

		internal void Add(Entity entity)
		{
			CheckIfInUpdateState();
			foreach (string tag in entity.tags)
				AddTag(entity, tag);
			foreach (var priority in prioritizedEntities)
			{
				if (priority.entities.Contains(entity))
					throw new EntityAlreadyAdded();
				foreach (var pair in priority.behaviors)
					if (entity.ContainsActiveBehavior(pair.Key))
						AddToUpdateBehavior(pair.Key, entity);
			}
			if (entity is DrawableEntity)
				flatDrawableEntities.Add(entity as DrawableEntity);
			prioritizedEntities[(int)entity.UpdatePriority].entities.Add(entity);
		}

		public class EntityAlreadyAdded : Exception {}

		private readonly List<DrawableEntity> flatDrawableEntities = new List<DrawableEntity>();

		internal void Remove(Entity entity)
		{
			CheckIfInUpdateState();
			foreach (string tag in entity.tags)
				RemoveTag(entity, tag);
			prioritizedEntities[(int)entity.UpdatePriority].entities.Remove(entity);
			foreach (var priority in prioritizedEntities)
				foreach (var behaviorEntities in priority.behaviors.Values)
					behaviorEntities.Remove(entity);
			var drawable = entity as DrawableEntity;
			if (drawable == null)
				return;
			flatDrawableEntities.Remove(drawable);
			CheckRenderlayerListToDeleteEntity(drawable);
		}

		private void CheckRenderlayerListToDeleteEntity(DrawableEntity entity)
		{
			for (int index = 0; index < negativeSortedDrawEntities.Count; index++)
				RemoveEntityIfInList(entity, negativeSortedDrawEntities[index]);
			for (int index = 0; index < positiveSortedDrawEntities.Count; index++)
				RemoveEntityIfInList(entity, positiveSortedDrawEntities[index]);
			foreach (var behavior in entity.drawBehaviors)
				if (unsortedDrawEntities.Keys.Contains(behavior))
					unsortedDrawEntities[behavior].Remove(entity);
		}

		private static void RemoveEntityIfInList(DrawableEntity entity,
			SortedDrawBehaviors sortedBehavior)
		{
			foreach (var behavior in entity.drawBehaviors)
				if (sortedBehavior.behaviors.Keys.Contains(behavior))
					sortedBehavior.behaviors[behavior].Remove(entity);
		}

		internal void AddTag(Entity entity, string tag)
		{
			List<Entity> entitiesWithTag;
			if (entityTags.TryGetValue(tag, out entitiesWithTag))
				entitiesWithTag.Add(entity);
			else
				entityTags.Add(tag, new List<Entity> { entity });
		}

		private readonly Dictionary<string, List<Entity>> entityTags =
			new Dictionary<string, List<Entity>>();

		internal void RemoveTag(Entity entity, string tag)
		{
			List<Entity> entitiesWithTag;
			if (entityTags.TryGetValue(tag, out entitiesWithTag))
				entitiesWithTag.Remove(entity);
		}

		public List<Entity> GetEntitiesWithTag(string searchTag)
		{
			List<Entity> entitiesWithTag;
			return entityTags.TryGetValue(searchTag, out entitiesWithTag)
				? entitiesWithTag : new List<Entity>();
		}

		public List<T> GetEntitiesOfType<T>() where T : Entity
		{
			return prioritizedEntities.SelectMany(priority => priority.entities).OfType<T>().ToList();
		}

		public int NumberOfEntities
		{
			get { return prioritizedEntities.Sum(priority => priority.entities.Count); }
		}

		public void Clear()
		{
			foreach (var priorizedEntity in prioritizedEntities)
				priorizedEntity.Clear();
			entityTags.Clear();
			flatDrawableEntities.Clear();
			negativeSortedDrawEntities.Clear();
			unsortedDrawEntities.Clear();
			positiveSortedDrawEntities.Clear();
		}

		public void UpdateAndDrawAllEntities(Action drawEverythingInCurrentLayer)
		{
			UpdateTimePassed();
			State = UpdateDrawState.RapidUpdate;
			while (rapidUpdateTimeAccumulator >= RapidUpdateTimeStep)
				RunRapidUpdateTick();
			State = UpdateDrawState.Update;
			while (updateTimeAccumulator >= UpdateTimeStep)
				RunUpdateTick();
			State = UpdateDrawState.Draw;
			RunDrawTick(drawEverythingInCurrentLayer);
			State = UpdateDrawState.Initialization;
		}

		private void UpdateTimePassed()
		{
			var thisMs = GlobalTime.Current.Milliseconds;
			EnsureFirstFrameRunsUpdateAndRapidUpdateAtLeastOnceButNotTooManyTimes(thisMs);
			var timePassed = (thisMs - lastMs) / 1000.0f;
			rapidUpdateTimeAccumulator += timePassed;
			updateTimeAccumulator += timePassed;
			lastMs = thisMs;
		}

		private void EnsureFirstFrameRunsUpdateAndRapidUpdateAtLeastOnceButNotTooManyTimes(long thisMs)
		{
			if (lastMs == 0)
				lastMs = thisMs - (long)(1000 * MathExtensions.Max(UpdateTimeStep, RapidUpdateTimeStep));
		}

		private long lastMs;
		private float rapidUpdateTimeAccumulator;
		private float updateTimeAccumulator;

		private void RunRapidUpdateTick()
		{
			rapidUpdateTimeAccumulator -= RapidUpdateTimeStep;
			float rapidDelta = RapidUpdateTimeStep * Time.SpeedFactor;
			if (rapidDelta == 0)
				return;
			foreach (var priority in prioritizedEntities)
				foreach (var entity in priority.entities.OfType<RapidUpdateable>())
					entity.RapidUpdate(rapidDelta);
		}

		private void RunUpdateTick()
		{
			updateTimeAccumulator -= UpdateTimeStep;
			Time.Delta = UpdateTimeStep * Time.SpeedFactor;
			if (Time.Delta == 0)
				return;
			Time.Total += Time.Delta;
			foreach (var entity in flatDrawableEntities)
				entity.InternalNextUpdateStarted();
			foreach (var priority in prioritizedEntities)
			{
				foreach (var pair in priority.behaviors.Where(pair => pair.Value.Count > 0))
					UpdateEntities(pair.Key, pair.Value);
				foreach (var entity in priority.entities.OfType<Updateable>())
					entity.Update();
				if (priority.delayedNewBehaviorsWhileUpdating.Count <= 0)
					continue;
				foreach (var pair in priority.delayedNewBehaviorsWhileUpdating)
					if (priority.behaviors.ContainsKey(pair.Key))
						priority.behaviors[pair.Key].AddRange(pair.Value);
					else
						priority.behaviors.Add(pair.Key, pair.Value);
				priority.delayedNewBehaviorsWhileUpdating.Clear();
			}
		}

		public UpdateDrawState State { get; private set; }

		private static void UpdateEntities(UpdateBehavior updateBehavior,
			IEnumerable<Entity> entities)
		{
			var filtered = updateBehavior as Filtered;
			updateBehavior.Update(filtered != null ? entities.Where(filtered.Filter).ToList() : entities);
		}

		private void RunDrawTick(Action drawEverythingInCurrentLayer)
		{
			CurrentDrawInterpolation = updateTimeAccumulator / UpdateTimeStep;
			foreach (var drawBehaviorEntities in negativeSortedDrawEntities)
			{
				foreach (var pair in drawBehaviorEntities.behaviors.Where(pair => pair.Value.Count > 0))
					pair.Key.Draw(pair.Value.Where(entity => entity.Visibility == Visibility.Show));
				drawEverythingInCurrentLayer();
			}
			foreach (var pair in unsortedDrawEntities.Where(pair => pair.Value.Count > 0))
				pair.Key.Draw(pair.Value.Where(entity => entity.Visibility == Visibility.Show));
			drawEverythingInCurrentLayer();
			foreach (var drawBehaviorEntities in positiveSortedDrawEntities)
			{
				foreach (var pair in drawBehaviorEntities.behaviors.Where(pair => pair.Value.Count > 0))
					pair.Key.Draw(pair.Value.Where(entity => entity.Visibility == Visibility.Show));
				drawEverythingInCurrentLayer();
			}
		}

		internal static float CurrentDrawInterpolation { get; private set; }

		/// <summary>
		/// Complete list of DrawBehaviors even if some are empty because they are in the lists below.
		/// </summary>
		private readonly Dictionary<DrawBehavior, List<DrawableEntity>> unsortedDrawEntities =
			new Dictionary<DrawBehavior, List<DrawableEntity>>();
		/// <summary>
		/// Outer list is for RenderLayer, each draw behavior has a list of entities to draw
		/// </summary>
		private readonly List<SortedDrawBehaviors> negativeSortedDrawEntities =
			new List<SortedDrawBehaviors>();
		private readonly List<SortedDrawBehaviors> positiveSortedDrawEntities =
			new List<SortedDrawBehaviors>();

		public T GetUpdateBehavior<T>() where T : UpdateBehavior
		{
			foreach (var priority in prioritizedEntities)
				foreach (var behavior in priority.delayedNewBehaviorsWhileUpdating.Keys.OfType<T>())
					return behavior;
			foreach (var priority in prioritizedEntities)
				foreach (var behavior in priority.behaviors.Keys.OfType<T>())
					return behavior;
			var newBehavior = behaviorResolver.ResolveUpdateBehavior(typeof(T)) as T;
			if (newBehavior == null)
				throw new UnableToResolveBehavior(typeof(T));
			prioritizedEntities[(int)newBehavior.priority].AddEntityToBehavior(newBehavior, null);
			return newBehavior;
		}

		internal void AddToUpdateBehavior(UpdateBehavior behavior, Entity entity)
		{
			prioritizedEntities[(int)behavior.priority].AddEntityToBehavior(behavior, entity);
		}

		public class UnableToResolveBehavior : Exception
		{
			public UnableToResolveBehavior(Type handlerType)
				: base(handlerType.ToString()) {}
		}

		public T GetDrawBehavior<T>() where T : class, DrawBehavior
		{
			foreach (var behavior in unsortedDrawEntities.Keys.OfType<T>())
				return behavior;
			var newBehavior = behaviorResolver.ResolveDrawBehavior(typeof(T)) as T;
			if (newBehavior == null)
				throw new UnableToResolveBehavior(typeof(T));
			unsortedDrawEntities.Add(newBehavior, new List<DrawableEntity>());
			return newBehavior;
		}

		internal void ChangeRenderLayer(DrawableEntity entity,
			IEnumerable<DrawBehavior> drawBehaviors)
		{
			foreach (var behavior in drawBehaviors)
			{
				RemoveFromDrawBehaviorList(entity, behavior);
				AddToDrawBehaviorList(entity, behavior);
			}
		}

		private void RemoveFromDrawBehaviorList(DrawableEntity entity, DrawBehavior drawBehavior)
		{
			foreach (var pair in unsortedDrawEntities.Where(pair => pair.Key == drawBehavior))
				pair.Value.Remove(entity);
			foreach (var drawBehaviorEntities in negativeSortedDrawEntities)
				foreach (var pair in drawBehaviorEntities.behaviors.Where(pair => pair.Key == drawBehavior))
					pair.Value.Remove(entity);
			foreach (var drawBehaviorEntities in positiveSortedDrawEntities)
				foreach (var pair in drawBehaviorEntities.behaviors.Where(pair => pair.Key == drawBehavior))
					pair.Value.Remove(entity);
		}

		internal void AddToDrawBehaviorList(DrawableEntity entity, DrawBehavior drawBehavior)
		{
			if (entity.RenderLayer == DrawableEntity.DefaultRenderLayer)
				unsortedDrawEntities[drawBehavior].Add(entity);
			else
				FindSpotOrCreateOne(entity.RenderLayer).Add(drawBehavior, entity);
		}

		private SortedDrawBehaviors FindSpotOrCreateOne(int renderLayer)
		{
			var sortedList = renderLayer < 0 ? negativeSortedDrawEntities : positiveSortedDrawEntities;
			int index = 0;
			for (; index < sortedList.Count; index++)
			{
				var layer = sortedList[index];
				if (layer.RenderLayer == renderLayer)
					return layer;
				if (layer.RenderLayer > renderLayer)
					break;
			}
			var newList = new SortedDrawBehaviors(renderLayer);
			sortedList.Insert(index, newList);
			return newList;
		}

		internal void ChangeEntityPriority(Entity entity, Priority priority)
		{
			prioritizedEntities[(int)entity.UpdatePriority].entities.Remove(entity);
			prioritizedEntities[(int)priority].entities.Add(entity);
		}

		internal void RemoveFromBehaviorList(UpdateBehavior behavior, Entity entity)
		{
			prioritizedEntities[(int)behavior.priority].RemoveEntityFromBehaviors(entity, behavior);
		}

		internal void CheckIfInUpdateState()
		{
			if (State == UpdateDrawState.Draw)
				throw new YouAreNotAllowedToSetAnEntityComponentInsideTheDrawLoop();
		}

		public class YouAreNotAllowedToSetAnEntityComponentInsideTheDrawLoop : Exception {}

		internal void CheckIfInDrawState()
		{
			if (State != UpdateDrawState.Draw)
				throw new YouAreNotAllowedToDrawOutsideOfTheDrawLoop();
		}

		public class YouAreNotAllowedToDrawOutsideOfTheDrawLoop : Exception {}

		public List<Entity> GetAllEntities()
		{
			var entities = new List<Entity>();
			foreach (var enity in prioritizedEntities)
				entities.AddRange(enity.entities);
			return entities;
		}
	}
}