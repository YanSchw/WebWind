using WebWind.Core;

namespace WebWind.Components;

public class VStack : Component
{

    public VStack()
    {
        JS.CreateDocumentElement("v-stack", Id);
    }

}