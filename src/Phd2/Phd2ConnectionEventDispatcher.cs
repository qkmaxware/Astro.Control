namespace Qkmaxware.Astro.Control {

public class Phd2ConnectionEventDispatcher {
    public delegate void Phd2VersionListener(Phd2VersionMessage version);
    public event Phd2VersionListener OnVersionRecieved = delegate {};
    internal void NotifyVersionRecieved(Phd2VersionMessage ver) {
        this.OnVersionRecieved?.Invoke(ver);
    } 

    public delegate void Phd2AppStateListener(Phd2AppStateMessage state);
    public event Phd2AppStateListener OnAppStateChanged = delegate {};
    internal void NotifyAppStateChanged(Phd2AppStateMessage msg) {
        this.OnAppStateChanged?.Invoke(msg);
    }

   public delegate void Phd2AlertMessageListener(Phd2AlertMessage msg);
   public event Phd2AlertMessageListener OnAlertReceived = delegate {};
   internal void NotifyAlertReceived(Phd2AlertMessage msg) {
       this.OnAlertReceived?.Invoke(msg);
   }

  public delegate void Phd2ConfigurationChangedMessageListener(Phd2ConfigurationChangeMessage msg);
  public event Phd2ConfigurationChangedMessageListener OnConfigurationChanged = delegate {};
  internal void NotifyConfigurationChanged(Phd2ConfigurationChangeMessage msg) {
      this.OnConfigurationChanged?.Invoke(msg);
  }

  public delegate void Phd2LockPositionSetMessageListener(Phd2LockPositionSetMessage msg);
  public event Phd2LockPositionSetMessageListener OnLockPositionSet = delegate {};
  internal void NotifyLockPositionSet(Phd2LockPositionSetMessage msg) {
      this.OnLockPositionSet?.Invoke(msg);
  }

  public delegate void Phd2LockPositionShiftLimitReachedMessageListener(Phd2LockPositionShiftLimitReachedMessage msg);
  public event Phd2LockPositionShiftLimitReachedMessageListener OnLockPositionShiftLimitReached = delegate {};
  internal void NotifyLockPositionShiftLimitReached(Phd2LockPositionShiftLimitReachedMessage msg) {
      this.OnLockPositionShiftLimitReached?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2LockPositionLostMessage(), node);
  // Events.NotifyLockPositionLost(message);
  public delegate void Phd2LockPositionLostMessageListener(Phd2LockPositionLostMessage msg);
  public event Phd2LockPositionLostMessageListener OnLockPositionLost = delegate {};
  internal void NotifyLockPositionLost(Phd2LockPositionLostMessage msg) {
      this.OnLockPositionLost?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2CalibratingMessage(), node);
  // Events.NotifyCalibrating(message);
  public delegate void Phd2CalibratingMessageListener(Phd2CalibratingMessage msg);
  public event Phd2CalibratingMessageListener OnCalibrating = delegate {};
  internal void NotifyCalibrating(Phd2CalibratingMessage msg) {
      this.OnCalibrating?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2CalibrationStartMessage(), node);
  // Events.NotifyCalibrationStart(message);
  public delegate void Phd2CalibrationStartMessageListener(Phd2CalibrationStartMessage msg);
  public event Phd2CalibrationStartMessageListener OnCalibrationStart = delegate {};
  internal void NotifyCalibrationStart(Phd2CalibrationStartMessage msg) {
      this.OnCalibrationStart?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2CalibrationCompleteMessage(), node);
  // Events.NotifyCalibrationComplete(message);
  public delegate void Phd2CalibrationCompleteMessageListener(Phd2CalibrationCompleteMessage msg);
  public event Phd2CalibrationCompleteMessageListener OnCalibrationComplete = delegate {};
  internal void NotifyCalibrationComplete(Phd2CalibrationCompleteMessage msg) {
      this.OnCalibrationComplete?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2StarSelectedMessage(), node);
  // Events.NotifyStarSelected(message);
  public delegate void Phd2StarSelectedMessageListener(Phd2StarSelectedMessage msg);
  public event Phd2StarSelectedMessageListener OnStarSelected = delegate {};
  internal void NotifyStarSelected(Phd2StarSelectedMessage msg) {
      this.OnStarSelected?.Invoke(msg);
  }

  // Usage:
  // var message = MapMsg2Obj(new Phd2StartGuidingMessage(), node);
  // Events.NotifyStartGuiding(message);
  public delegate void Phd2StartGuidingMessageListener(Phd2StartGuidingMessage msg);
  public event Phd2StartGuidingMessageListener OnStartGuiding = delegate {};
  internal void NotifyStartGuiding(Phd2StartGuidingMessage msg) {
      this.OnStartGuiding?.Invoke(msg);
  }

    // Usage:
    // var message = MapMsg2Obj(new Phd2PausedGuidingMessage(), node);
    // Events.NotifyPausedGuiding(message);
    public delegate void Phd2PausedGuidingMessageListener(Phd2PausedGuidingMessage msg);
    public event Phd2PausedGuidingMessageListener OnPausedGuiding = delegate {};
    internal void NotifyPausedGuiding(Phd2PausedGuidingMessage msg) {
        this.OnPausedGuiding?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2CalibrationFailedMessage(), node);
    // Events.NotifyCalibrationFailed(message);
    public delegate void Phd2CalibrationFailedMessageListener(Phd2CalibrationFailedMessage msg);
    public event Phd2CalibrationFailedMessageListener OnCalibrationFailed = delegate {};
    internal void NotifyCalibrationFailed(Phd2CalibrationFailedMessage msg) {
        this.OnCalibrationFailed?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2CalibrationDataFlippedMessage(), node);
    // Events.NotifyCalibrationDataFlipped(message);
    public delegate void Phd2CalibrationDataFlippedMessageListener(Phd2CalibrationDataFlippedMessage msg);
    public event Phd2CalibrationDataFlippedMessageListener OnCalibrationDataFlipped = delegate {};
    internal void NotifyCalibrationDataFlipped(Phd2CalibrationDataFlippedMessage msg) {
        this.OnCalibrationDataFlipped?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2LoopingExposuresIterationMessage(), node);
    // Events.NotifyLoopingExposuresIteration(message);
    public delegate void Phd2LoopingExposuresIterationMessageListener(Phd2LoopingExposuresIterationMessage msg);
    public event Phd2LoopingExposuresIterationMessageListener OnLoopingExposuresIteration = delegate {};
    internal void NotifyLoopingExposuresIteration(Phd2LoopingExposuresIterationMessage msg) {
        this.OnLoopingExposuresIteration?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2LoopingExposuresStoppedMessage(), node);
    // Events.NotifyLoopingExposuresStopped(message);
    public delegate void Phd2LoopingExposuresStoppedMessageListener(Phd2LoopingExposuresStoppedMessage msg);
    public event Phd2LoopingExposuresStoppedMessageListener OnLoopingExposuresStopped = delegate {};
    internal void NotifyLoopingExposuresStopped(Phd2LoopingExposuresStoppedMessage msg) {
        this.OnLoopingExposuresStopped?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2SettleBeginMessage(), node);
    // Events.NotifySettleBegin(message);
    public delegate void Phd2SettleBeginMessageListener(Phd2SettleBeginMessage msg);
    public event Phd2SettleBeginMessageListener OnSettleBegin = delegate {};
    internal void NotifySettleBegin(Phd2SettleBeginMessage msg) {
        this.OnSettleBegin?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2SettlingMessage(), node);
    // Events.NotifySettling(message);
    public delegate void Phd2SettlingMessageListener(Phd2SettlingMessage msg);
    public event Phd2SettlingMessageListener OnSettling = delegate {};
    internal void NotifySettling(Phd2SettlingMessage msg) {
        this.OnSettling?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2SettleDoneMessage(), node);
    // Events.NotifySettleDone(message);
    public delegate void Phd2SettleDoneMessageListener(Phd2SettleDoneMessage msg);
    public event Phd2SettleDoneMessageListener OnSettleDone = delegate {};
    internal void NotifySettleDone(Phd2SettleDoneMessage msg) {
        this.OnSettleDone?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2StarLostMessage(), node);
    // Events.NotifyStarLost(message);
    public delegate void Phd2StarLostMessageListener(Phd2StarLostMessage msg);
    public event Phd2StarLostMessageListener OnStarLost = delegate {};
    internal void NotifyStarLost(Phd2StarLostMessage msg) {
        this.OnStarLost?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2StoppedGuidingMessage(), node);
    // Events.NotifyStoppedGuiding(message);
    public delegate void Phd2StoppedGuidingMessageListener(Phd2StoppedGuidingMessage msg);
    public event Phd2StoppedGuidingMessageListener OnStoppedGuiding = delegate {};
    internal void NotifyStoppedGuiding(Phd2StoppedGuidingMessage msg) {
        this.OnStoppedGuiding?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2ResumedGuidingMessage(), node);
    // Events.NotifyResumedGuiding(message);
    public delegate void Phd2ResumedGuidingMessageListener(Phd2ResumedGuidingMessage msg);
    public event Phd2ResumedGuidingMessageListener OnResumedGuiding = delegate {};
    internal void NotifyResumedGuiding(Phd2ResumedGuidingMessage msg) {
        this.OnResumedGuiding?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2GuideStepMessage(), node);
    // Events.NotifyGuideStep(message);
    public delegate void Phd2GuideStepMessageListener(Phd2GuideStepMessage msg);
    public event Phd2GuideStepMessageListener OnGuideStep = delegate {};
    internal void NotifyGuideStep(Phd2GuideStepMessage msg) {
        this.OnGuideStep?.Invoke(msg);
    }

    // Usage:
    // var message = MapMsg2Obj(new Phd2GuidingDitheredMessage(), node);
    // Events.NotifyGuidingDithered(message);
    public delegate void Phd2GuidingDitheredListener(Phd2GuidingDitheredMessage msg);
    public event Phd2GuidingDitheredListener OnGuidingDithered = delegate {};
    internal void NotifyGuidingDithered(Phd2GuidingDitheredMessage msg) {
        this.OnGuidingDithered?.Invoke(msg);
    }
    
    // Usage:
    // var message = MapMsg2Obj(new Phd2GuideParametreChangeMessage(), node);
    // Events.NotifyGuideParametreChange(message);
    public delegate void Phd2GuideParametreChangeMessageListener(Phd2GuideParametreChangeMessage msg);
    public event Phd2GuideParametreChangeMessageListener OnGuideParametreChange = delegate {};
    internal void NotifyGuideParametreChange(Phd2GuideParametreChangeMessage msg) {
        this.OnGuideParametreChange?.Invoke(msg);
    }
}

}