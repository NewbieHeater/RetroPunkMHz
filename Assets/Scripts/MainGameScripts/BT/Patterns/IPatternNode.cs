public interface IPatternNode
{
    void NodeStart(BTContext ctx);
    NodeStatus Tick(BTContext ctx);
    void NodeStop(BTContext ctx, NodeStatus result);
}
