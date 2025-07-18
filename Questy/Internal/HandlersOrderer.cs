namespace Questy.Internal;

internal static class HandlersOrderer
{
    public static IList<object> Prioritize<TRequest>(IList<object> handlers, TRequest request)
        where TRequest : notnull
    {
        if (handlers.Count < 2)
        {
            return handlers;
        }

        ObjectDetails requestObjectDetails = new(request);
        List<ObjectDetails> handlerObjectsDetails = handlers.Select(static s => new ObjectDetails(s)).ToList();

        ObjectDetails[] uniqueHandlers = RemoveOverridden(handlerObjectsDetails).ToArray();
        Array.Sort(uniqueHandlers, requestObjectDetails);

        return uniqueHandlers.Select(static s => s.Value).ToList();
    }

    private static IEnumerable<ObjectDetails> RemoveOverridden(IList<ObjectDetails> handlersData)
    {
        for (int i = 0; i < handlersData.Count - 1; i++)
        {
            for (int j = i + 1; j < handlersData.Count; j++)
            {
                if (handlersData[i].IsOverridden || handlersData[j].IsOverridden)
                {
                    continue;
                }

                if (handlersData[i].Type.IsAssignableFrom(handlersData[j].Type))
                {
                    handlersData[i].IsOverridden = true;
                }
                else if (handlersData[j].Type.IsAssignableFrom(handlersData[i].Type))
                {
                    handlersData[j].IsOverridden = true;
                }
            }
        }

        return handlersData.Where(static w => !w.IsOverridden);
    }
}