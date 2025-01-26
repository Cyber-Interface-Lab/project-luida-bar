using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public interface ILoggerFactory
    {
        ILogger Create(IProgramStatus programStatus);
    }

    public interface IRunnerOptions
    {
        bool isDebug { get; }
        string pauseFrameKey { get; }
        IExternalCallerOptions externalCallerOptions { get; }
        bool isRayDraw { get; }
    }

    public interface IExternalCallerOptions
    {
        event Handler OnChangeLimit;
        string url { get; }
        EmulateClasses.CallExternalRateLimit rateLimit { get; }
    }

    public interface IPlayerOptions
    {
        bool isFirstPersonView { get; set; }
        void SetPersonViewChangeable(bool canChange);
    }

    public interface IProductOptions
    {
        bool IsPublicProduct(string productId);
        string GetProductName(string productId);
        (int, int) GetProductAmount(string productId); //(plus,minus)
        void SetProductAmount(string productId, int plus, int minus);
    }

    public interface IEngineApplyBuilder
    {
        EmulateClasses.IPlayerHandleFactory BuildFactory(
            Components.CSEmulatorItemHandler csItemHandler,
            Jint.Engine engine
        );
    }

    public interface ISerializedPlayerStorage
    {
        void SavePlayerStorage(string serialized);
        string LoadPlayerStorage();
    }
}
