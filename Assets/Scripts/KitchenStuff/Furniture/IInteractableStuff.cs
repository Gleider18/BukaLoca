using System.Collections;

namespace KitchenStuff
{
    public interface IInteractableStuff
    {
        IEnumerator AnimationGoing();
        void ChangeStuffState();
    }
}
