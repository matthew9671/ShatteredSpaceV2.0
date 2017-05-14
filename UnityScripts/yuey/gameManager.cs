// Yao Yue's version of the gameManager_t that implements the timeline
// Will eventually be combined with others' versions
// This would only work in Unity

public static class gameManager_t
{
    public enum inputMode_t {NONE, ATTACK, MOVE, SPMOVE, WEAPON};
    public Stack<action_t> actions;

    public static void init_planning()
    // Initialize the action stack for planning
    // Also a good place to initialize your timeline
    {
        actions = new Stack<action_t>();
        // The first action doesn't have any movement
        // (But it could have special movement)
        actions.Push(new action(Vector2.Zero));
    }

    public void add_on_timeline(inputMode_t input)
    // Called when the user adds an input of type input on the timeline
    {
        // Look at the top element on the stack, 
        // figure out what the relavant parameters are
    }

    public void cancel_on_timeline()
    {
    }

    // Methods for testing
    public void add_grenade()
    {}
}