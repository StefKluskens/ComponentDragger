//Tool created by Stef Kluskens

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SK.Tools.ComponentDragger
{
    [InitializeOnLoad]
    public static class ComponentDragger
    {
        private enum ComponentTransferMode
        {
            Copy,
            Move
        }

        static ComponentDragger()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            Event currentEvent = Event.current;

            if (!DragEventValid(currentEvent, selectionRect))
            {
                return;
            }

            GameObject targetObject = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (targetObject == null)
            {
                return;
            }

            if (DragAndDrop.objectReferences.Length == 0)
            {
                return;
            }

            Component draggedComponent = DragAndDrop.objectReferences[0] as Component;

            if (!DraggedComponentValid(draggedComponent, targetObject))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEvent.type == EventType.DragPerform)
            {
                OnEndDrag(currentEvent, draggedComponent, targetObject);
            }
        }

        private static bool DragEventValid(Event currentEvent, Rect selectionRect)
        {
            return (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
                && selectionRect.Contains(currentEvent.mousePosition);
        }

        private static bool DraggedComponentValid(Component draggedComponent, GameObject targetObject)
        {
            return draggedComponent != null && draggedComponent.gameObject != targetObject;
        }

        private static void OnEndDrag(Event currentEvent, Component draggedComponent, GameObject targetObject)
        {
            DragAndDrop.AcceptDrag();

            CreateContextMenu(draggedComponent, targetObject);
            
            currentEvent.Use();
        }

        private static void CreateContextMenu(Component draggedComponent, GameObject targetObject)
        {
            GenericMenu menu = new();

            menu.AddItem(
                new GUIContent($"Copy {draggedComponent.GetType().ToString()} to {targetObject.name}"),
                false,
                () => TransferComponent(draggedComponent, targetObject, ComponentTransferMode.Copy));

            menu.AddItem(
                new GUIContent($"Move {draggedComponent.GetType().ToString()} to {targetObject.name}"),
                false,
                () => TransferComponent(draggedComponent, targetObject, ComponentTransferMode.Move));

            menu.ShowAsContext();
        }

        private static void TransferComponent(Component draggedComponent, GameObject targetObject, ComponentTransferMode transferMode)
        {
            if (draggedComponent == null || targetObject == null)
            {
                return;
            }

            string undoLabel = $"{transferMode} Component";
            Undo.RegisterCompleteObjectUndo(targetObject, undoLabel);

            if (transferMode == ComponentTransferMode.Move)
            {
                Undo.RegisterCompleteObjectUndo(draggedComponent.gameObject, undoLabel);
            }

            ComponentUtility.CopyComponent(draggedComponent);
            ComponentUtility.PasteComponentAsNew(targetObject);

            if (transferMode == ComponentTransferMode.Move)
            {
                Undo.DestroyObjectImmediate(draggedComponent);
            }

            EditorUtility.SetDirty(targetObject);
        }
    }
}
