﻿using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Zenject;

[RequireComponent(typeof(Collider2D))]
public class ItemTakeable : MonoBehaviour
{

	private int itemValue;

	[Tooltip("Applicable to TAG: Money or Shots")]
	[SerializeField] private int itemValueMin = 20;
	[SerializeField] private int itemValueMax = 500;
	[SerializeField] private GameObject prefabTakenEffect;

	[Inject] PlayerStatSetter_Health setterHealth;
	[Inject] PlayerStatSetter_Lives setterLives;
	[Inject] PlayerStatSetter_Money setterMoney;
	[Inject] PlayerStatSetter_Scrolls setterScrolls;
	[Inject] PlayerStatSetter_Shots setterShots;
	[Inject] Timer_Setter setterTimer;

	[Inject] Instantiator instantiator;

	private void Awake() {
		if(TagUtil.IsTagItemMultiValue(this.gameObject.tag)) {
			itemValue = Random.Range(itemValueMin, itemValueMax);
		} else {
			itemValue = 1;
		}
	}

	private void Start() {
		this.OnTriggerEnter2DAsObservable()
			.Where(otherCollider2D => TagUtil.IsTagPlayer(otherCollider2D.tag, false))
			.Subscribe(_ => {
				AddItemToPlayerStats(this.gameObject.tag);
				DestroyItem();
			})
			.AddTo(this);

		this.OnCollisionEnter2DAsObservable()
			.Where(collision2D => TagUtil.IsTagPlayer(collision2D.gameObject.tag, false))
			.Subscribe(_ => {
				AddItemToPlayerStats(this.gameObject.tag);
				DestroyItem();
			})
			.AddTo(this);
	}

	private void AddItemToPlayerStats(string tag) {
		if(tag.Equals(OBJECT_TAG.Item_Health.ToString())) {
			setterHealth.AddHealth(itemValue);
		}
		else if(tag.Equals(OBJECT_TAG.Item_Life.ToString())) {
			setterLives.AddLives(itemValue);
		}
		else if(tag.Equals(OBJECT_TAG.Item_Money.ToString())) {
			setterMoney.AddMoney(itemValue);
		}
		else if(tag.Equals(OBJECT_TAG.Item_Scroll.ToString())) {
			setterScrolls.AddScroll();
		}
		else if(tag.Equals(OBJECT_TAG.Item_Shots.ToString())) {
			setterShots.AddShots(itemValue);
		}
		else if(tag.Equals(OBJECT_TAG.Item_Clock.ToString())) {
			setterTimer.AddToCountdown(itemValue);
		}
	}

	private void DestroyItem() {
		if(prefabTakenEffect != null) {
			GameObject takenEffect = Instantiate(prefabTakenEffect, this.gameObject.transform.position,
				this.gameObject.transform.rotation);

			instantiator.InjectPrefab(takenEffect);

			if(TagUtil.IsTagItemMultiValue(this.gameObject.tag)) {
				TextMeshOutliner textOutliner = takenEffect.GetComponent<TextMeshOutliner>();
				if(textOutliner != null) {
					textOutliner.SetText("+" + itemValue.ToString());
				} else {
					LogUtil.PrintWarning(gameObject, GetType(), "No TextMeshOutliner in the prefab.");
				}
			}
		}

		Destroy(this.gameObject);
	}

}

