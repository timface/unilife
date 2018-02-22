using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EstateJobSpriteController : MonoBehaviour {

	//This dictionary is used to track which GO is used to render the fixture
	Dictionary<EstateJob, GameObject> estateJobGameObjectMap;

	FixtureSpriteController fsc;

	World world {
		get { return WorldController.Instance.World; }
	}


	// Use this for initialization
	void Start () {
		estateJobGameObjectMap = new Dictionary<EstateJob, GameObject> ();

		fsc = GameObject.FindObjectOfType<FixtureSpriteController> ();

		world.estateJobManager.RegisterJobCreatedCallback (OnJobCreated);
		//We do this for any fixtures that may already exist in the world(via loading or otherwise) so that we render those
		foreach (EstateJob job in world.estateJobManager.GetAllJobs()) {
			OnJobCreated (job);
		}
	}

	public void OnJobCreated(EstateJob job){

        if (job.JobType != EstateJobType.BUILD)
            return;

        Debug.Log("Job is being created");
        GameObject job_go = new GameObject ();

		//Add data and UI to map
		estateJobGameObjectMap.Add (job, job_go);

		//Set up general info about the GO
		job_go.name = job.Fixture.ObjectType + "_" + job.jobTile.X + "_" + job.jobTile.Y;
		job_go.transform.position = new Vector3 (job.jobTile.X + ((job.Fixture.Width - 1) / 2f), job.jobTile.Y + ((job.Fixture.Height - 1) / 2f));
		job_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = job_go.AddComponent<SpriteRenderer> ();
		sr.sprite = fsc.GetSpriteForFixture (job.Fixture);
		sr.sortingLayerName = "Fixture";

		sr.color = new Color (0.5f, 1f, 0.5f, 0.3f);

        //TODO: Implement these
        //job.RegisterOnChanged (OnFixtureChanged);
        //job.RegisterOnRemoved (OnFixtureRemoved);
        job.RegisterJobCompletedCallback(OnJobCompleted);
	}

    public void OnJobCompleted(EstateJob job)
    {
        if (estateJobGameObjectMap.ContainsKey(job))
        {
            GameObject jobGo = estateJobGameObjectMap[job];
            Destroy(jobGo);
            estateJobGameObjectMap.Remove(job);
        } else
        {
            Debug.LogError("EJSC::OnJobCompleted - Trying to remove a jobsprite not in the dictionary");
        }

    }

//	void OnFixtureChanged (Ea fixt){
//		if (!estateJobGameObjectMap.ContainsKey (fixt)) {
//			Debug.LogError ("Trying to change visuals of a fixture not in the list? ");
//			return;
//		}
//
//		GameObject job_go = estateJobGameObjectMap [fixt];
//
//		job_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForFixture (fixt);
//	}
//
//	void OnFixtureRemoved (Fixture fixt){
//		if (!estateJobGameObjectMap.ContainsKey (fixt)) {
//			Debug.LogError ("Trying to remove a fixture not in the list? ");
//			return;
//		}
//
//		GameObject job_go = estateJobGameObjectMap [fixt];
//		Destroy (job_go);
//		estateJobGameObjectMap.Remove (fixt);
//
//	}
}