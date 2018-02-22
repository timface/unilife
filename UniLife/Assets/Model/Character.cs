using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CharacterRole { BUILDER, JANITOR, GARDERNER, ACADEMIC, ADMIN, SUPPORT, STUDENT }

public class Character {

	public CharacterRole CharRole { get; protected set; }
	string name;

	public float X {
		get {
			if (nextTile == null)
				return CurrTile.X;

			return Mathf.Lerp (CurrTile.X, nextTile.X, movementPercentage);
		}
	}

	public float Y {
		get {
			if (nextTile == null)
				return CurrTile.Y;

			return Mathf.Lerp (CurrTile.Y, nextTile.Y, movementPercentage);
		}
	}

	Tile _currTile;
	public Tile CurrTile {
		get { return _currTile; }
		protected set {
			if (_currTile != null) {
				_currTile.Characters.Remove (this);
			}

			_currTile = value;
			_currTile.Characters.Add (this);
		}
	}

	Tile _destTile;
	Tile DestTile {
		get { return _destTile; }
		set {
			if(_destTile != value) {
				_destTile = value;
				pathAstar = null;
			}
		}
	}

	Tile nextTile;
	PathAstar pathAstar;
	float movementPercentage;

	float speed = 3f;
	float jobSearchCoolDown = 0;

	Action<Character> characterOnChangedCallback;

	EstateJob myJob;

    HaulableItem item = null; 


	public Character(){
	}

	public Character(Tile tile, CharacterRole charRole){
		CurrTile = DestTile = nextTile = tile;

		this.CharRole = charRole;
	}

	void GetNewJob() {
		myJob = World.Current.estateJobManager.GiveJob (CurrTile, this);
		if (myJob == null)
			return;

		DestTile = myJob.jobTile;
		myJob.RegisterJobStoppedCallback (OnJobStopped);

		pathAstar = new PathAstar (World.Current, CurrTile, DestTile);
		if (pathAstar.Length () == 0 || pathAstar == null) {
			Debug.LogError ("Character::GetNewJob - PathAstar couldnt find a path to job site!");
			AbandonJob ();
			DestTile = CurrTile;
		}
	}


	void UpdateDoJob(float deltaTime){
		jobSearchCoolDown -= deltaTime;
		if (myJob == null) {
			if (jobSearchCoolDown > 0)
				return;

			GetNewJob ();

			if (myJob == null) {
				jobSearchCoolDown = UnityEngine.Random.Range (0.5f, 0.1f);
				DestTile = CurrTile;
				return;
			}
		}

        if (myJob.JobType != EstateJobType.HAUL)
        {
            DestTile = myJob.jobTile;
        }
        else
        {
            if (item == null)
            {
                DestTile = myJob.pickupTile;
                //Debug.Log("Should be heading to pick up at: " + DestTile.X + ":" + DestTile.Y);
            }
            else
                DestTile = myJob.jobTile;
        }

        if(CurrTile == myJob.pickupTile && item == null)
        {
            PickUpHaulableItem();
        }
        if (CurrTile == myJob.jobTile)
        {
            //Debug.Log("At job site");
            myJob.WorkJob(deltaTime);
        }
	}

	public void AbandonJob() {
		nextTile = DestTile = CurrTile;
		World.Current.estateJobManager.AddJob (myJob);
		myJob = null;
	}

	void UpdateDoMovement(float deltaTime) {
		if (CurrTile == DestTile) {
			pathAstar = null;
			return;
		}

		if (nextTile == null || nextTile == CurrTile) {
			if (pathAstar == null || pathAstar.Length () == 0) {
                Debug.Log("Creating new path");
				pathAstar = new PathAstar (World.Current, CurrTile, DestTile);
				if (pathAstar.Length () == 0) {
					Debug.LogError ("Character::UpdateDoMovement - PathAstar couldnt find a path to job site!");
					AbandonJob ();
                    DestTile = CurrTile;
					return;
				}

				nextTile = pathAstar.Dequeue ();
			}
			nextTile = pathAstar.Dequeue ();
            if (nextTile == CurrTile)
            {
                //Debug.LogError("Character::UpdateDoMovement - nextTIle is CurrTile?");
                return;
            }
		}

		float distToTravel = Mathf.Sqrt (Mathf.Pow (CurrTile.X - nextTile.X, 2) + Mathf.Pow (CurrTile.Y - nextTile.Y, 2));


		float distThisFrame = speed / nextTile.movementCost * deltaTime;

		float percThisFrame = distThisFrame / distToTravel;

		movementPercentage += percThisFrame;

		if (movementPercentage >= 1) {
			CurrTile = nextTile;
			movementPercentage = movementPercentage - 1;
		}

	}

	public void Update(float deltaTime){
		UpdateDoJob (deltaTime);
		UpdateDoMovement (deltaTime);

		if (characterOnChangedCallback != null)
			characterOnChangedCallback (this);
	}

	void OnJobStopped(EstateJob job){
		job.UnregisterJobStoppedCallback (OnJobStopped);
		if(job != myJob){
			Debug.LogError ("Character:: OnJobStopped - char being told about job that isnt theirs. Something wasnt unregistered good and stuff");
			return;
		}
		myJob = null;
	}

    public static void DeliverGoods(EstateJob job)
    {
        job.Fixture.RecieveContents(job.character.item.Contents, job.character.item.ContentAmount);
        job.character.item = null;
    }

    void PickUpHaulableItem()
    {
        item = CurrTile.haulableItem;//GiveItem();
        //Debug.Log(CurrTile.haulableItem);
        CurrTile.haulableItem.OnRemoved(CurrTile.haulableItem);
       // Debug.Log("Picked up thingo, item is now: " + item.Contents);
    }

	public void RegisterOnChangeCallback(Action<Character> cbFunc){
		characterOnChangedCallback += cbFunc;
	}

	public void unRegisterOnChangeCallback(Action<Character> cbFunc){
		characterOnChangedCallback -= cbFunc;
	}
}


