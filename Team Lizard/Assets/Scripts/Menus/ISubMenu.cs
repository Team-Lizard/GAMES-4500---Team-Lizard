using UnityEngine;
using UnityEngine.UIElements;

namespace Menus
{
    public interface ISubMenu
    {

        /// <summary>
        /// gets the visualTreeAsset associated with this UI
        /// </summary>
        /// <returns> read the summary :| </returns>
        public VisualTreeAsset GetVisualTreeAsset();

        /// <summary>
        /// Builds any UI elements that are created via C#
        /// </summary>
        /// <param name="root"> the root node containing all the new UI </param>
        public void Build(VisualElement root);
    }
}
