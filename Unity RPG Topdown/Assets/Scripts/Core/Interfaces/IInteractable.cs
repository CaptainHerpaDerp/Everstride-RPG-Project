using System;
using Core.Enums;

namespace Core.Interfaces
{
    public interface IInteractable  
    {
        void Interact(object playerObject, Action<ActionType> modifyInteractableLabel);
    }
}