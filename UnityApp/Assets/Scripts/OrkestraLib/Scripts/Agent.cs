namespace OrkestraLib
{
    public class Agent
    {
        public EventRegistry Handlers { get; private set; }

        public string AgentID { get; private set; }

        public Agent(string id)
        {
            Handlers = new EventRegistry();
            AgentID = id;
        }
    }
}