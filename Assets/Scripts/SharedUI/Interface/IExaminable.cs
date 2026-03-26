using FirstPersonPlayer.Tools.Interface;

namespace SharedUI.Interface
{
    public interface IExaminable
    {
        public void OnFinishExamining();

        public bool ExaminableWithRuntimeTool(IRuntimeTool tool);
    }
}
