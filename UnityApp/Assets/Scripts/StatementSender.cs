using UnityEngine;
using TinCan;
using TinCan.LRSResponses;
using System;

public class StatementSender
{
    public string _actor;
    public string _verb;
    public string _definition;
    public int _value = 0;

    private RemoteLRS lrs;

    // Use this for initialization
    public StatementSender()
    {
        string Endpoint = ""; // Endpoint of the learninglocker service here
        string Key = ""; // Key of the learninglocker service here
        string Secret = ""; // Secret of the learninglocker service here

        if (Endpoint != "" && Key != "" && Secret != "") {
            lrs = new RemoteLRS(
                Endpoint,
                Key,
                Secret
            );
        }
    }

    public void SendStatement()
    {
        // Build out Actor details
        Agent actor = new Agent();
        actor.mbox = "mailto:" + _actor.Replace(" ", "") + "@email.com";
        actor.name = _actor;

        // Build out Verb details
        Verb verb = new Verb();
        verb.id = new Uri("http://www.example.com/" + _verb.Replace(" ", ""));
        verb.display = new LanguageMap();
        verb.display.Add("en-US", _verb);

        // Build out Activity details
        Activity activity = new Activity();
        activity.id = new Uri("http://www.example.com/" + _definition.Replace(" ", "")).ToString();

        // Build out Activity Definition details
        ActivityDefinition activityDefinition = new ActivityDefinition();
        activityDefinition.description = new LanguageMap();
        activityDefinition.name = new LanguageMap();
        activityDefinition.name.Add("en-US", (_definition));
        activity.definition = activityDefinition;

        Result result = new Result();
        Score score = new Score();

        score.raw = _value;
        result.score = score;

        // Build out full Statement details
        Statement statement = new Statement();
        statement.actor = actor;
        statement.verb = verb;
        statement.target = activity;
        statement.result = result;

        if (lrs != null) {
            // Send the statement
            StatementLRSResponse lrsResponse = lrs.SaveStatement(statement);
            if (lrsResponse.success) //Success
            {
                // Debug.Log("Statement saved: " + lrsResponse.content.id);
            }
            else // Failure
            {
                Debug.Log("Statement Failed: " + lrsResponse.errMsg);
            }
        }
    }
}