namespace PollySim.Runner.Responder
{
    public interface IResponder
    {
        HttpResponseMessage Respond(DateTime startTime);
    }
}
