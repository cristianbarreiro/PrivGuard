using PrivLock.Application.Services;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Xunit;

namespace PrivLock.Application.Tests;

public sealed class ApplicationMutationGuardTests
{
    private static (OriginalPolicyState standardPolicy, OriginalDeviceState secureDevice) CreateCamResources()
    {
        var standardPolicy = PrivacyRecoveryTestData.Policy(
            "cam-policy-1",
            valueExisted: false,
            originalValue: null,
            layer: ProtectionLayer.Standard,
            target: BlockTarget.Camera);
        var secureDevice = PrivacyRecoveryTestData.Device("cam-dev-1", target: BlockTarget.Camera);
        return (standardPolicy, secureDevice);
    }

    // =========================================================================
    // 1. EnableStandardProtection with Standard Unknown: rejects, never calls provider
    // =========================================================================
    [Fact]
    public async Task Scenario01_EnableStandardProtection_WhenStandardUnknown_RejectsMutation_AndNeverCallsProvider()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Unknown,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = false,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableStandardProtectionAsync(BlockTarget.Camera);

        Assert.False(result.Success);
        Assert.Contains("unverified or unknown", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, deps.StandardEnableCalls);
        Assert.Null(store.Current);
    }

    // =========================================================================
    // 2. EnableStandardProtection with Standard Inactive verified and Secure Unknown:
    //    Standard executes; Secure is untouched.
    // =========================================================================
    [Fact]
    public async Task Scenario02_EnableStandardProtection_WhenStandardInactiveVerified_AndSecureUnknown_AppliesStandard_LeavesSecureUntouched()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.BeforeStandardEnableReturn = () =>
        {
            platform.SetObservation(stdPolicy.ResourceId, PrivacyResourceObservationKind.MatchesProtected);
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableStandardProtectionAsync(BlockTarget.Camera);

        Assert.True(result.Success);
        Assert.Equal(1, deps.StandardEnableCalls);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.NotNull(store.Current);
    }

    // =========================================================================
    // 3. EnableSecureProtection with Secure Unknown:
    //    Does not call secure provider or start new secure session.
    // =========================================================================
    [Fact]
    public async Task Scenario03_EnableSecureProtection_WhenSecureUnknown_RejectsMutation_NeverCallsProvider_NorOpensSession()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableSecureProtectionAsync(BlockTarget.Camera);

        Assert.False(result.Success);
        Assert.Contains("unverified or unknown", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.Null(store.Current);
    }

    // =========================================================================
    // 4. EnableSecureProtection with Secure unverified even if Available: rejects
    // =========================================================================
    [Fact]
    public async Task Scenario04_EnableSecureProtection_WhenSecureUnverified_EvenIfAvailable_RejectsMutation()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableSecureProtectionAsync(BlockTarget.Camera);

        Assert.False(result.Success);
        Assert.Contains("unverified or unknown", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.Null(store.Current);
    }

    // =========================================================================
    // 5. EnableSecureProtection with Secure Active: idempotent, without reapplying
    // =========================================================================
    [Fact]
    public async Task Scenario05_EnableSecureProtection_WhenSecureActive_IsIdempotent_DoesNotRemutate()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Active,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        // Put an active session in store for this device
        var existingSession = PrivacyRecoveryTestData.ActiveSession(secDev);
        store.Save(existingSession);
        platform.SetObservation(secDev.ResourceId, PrivacyResourceObservationKind.MatchesProtected);

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableSecureProtectionAsync(BlockTarget.Camera);

        Assert.True(result.Success);
        Assert.Equal(0, deps.SecureEnableCalls); // Native provider was not re-called
    }

    // =========================================================================
    // 6. BlockDeviceAsync with Advanced enabled and Secure Unknown:
    //    Applies Standard, defers Secure, outcome and DesiredState documented.
    // =========================================================================
    [Fact]
    public async Task Scenario06_BlockDeviceAsync_WithAdvancedEnabled_AndSecureUnknown_AppliesStandard_DefersSecure_PreservesAdvancedSetting()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.BeforeStandardEnableReturn = () =>
        {
            platform.SetObservation(stdPolicy.ResourceId, PrivacyResourceObservationKind.MatchesProtected);
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        // Precondition: user has Advanced Protection enabled globally
        var desired = deps.Load();
        desired.AdvancedProtectionEnabled = true;
        deps.Save(desired);

        var result = await service.BlockDeviceAsync(BlockTarget.Camera);

        // Standard applied successfully, Secure deferred with explicit detail
        Assert.True(result.Success);
        Assert.Equal(1, deps.StandardEnableCalls);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.Contains(result.Details, d => d.DeviceId == "Secure-Camera" && !d.Success);

        // AdvancedProtectionEnabled preference remains intact!
        var afterDesired = deps.Load();
        Assert.True(afterDesired.AdvancedProtectionEnabled);
        Assert.Equal(StandardProtectionState.Active, afterDesired.CameraStandard);
    }

    // =========================================================================
    // 7. SetAdvancedProtectionAsync(true) with Secure Unknown:
    //    Does not mutate Secure nor claims protection applied.
    // =========================================================================
    [Fact]
    public async Task Scenario07_SetAdvancedProtectionTrue_WhenSecureUnknown_DoesNotMutateSecure_NorClaimsProtectionApplied()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.SetAdvancedProtectionAsync(true);

        Assert.True(result.Success);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.Contains(result.Details, d => d.DeviceId == "Secure-Camera" && !d.Success);
        var savedDesired = deps.Load();
        Assert.True(savedDesired.AdvancedProtectionEnabled);
    }

    // =========================================================================
    // 8. SetAdvancedProtectionAsync(false) with Secure Unknown and WAL-confirmed resources:
    //    Restores only those resources via PrivacySessionService.
    // =========================================================================
    [Fact]
    public async Task Scenario08_SetAdvancedProtectionFalse_WhenSecureUnknown_WithWalOwnedResources_RestoresOnlyOwnedViaPrivacySession()
    {
        var cameraSecurePolicy = PrivacyRecoveryTestData.Policy(
            "cam-sec-policy-1",
            valueExisted: true,
            originalValue: "1",
            layer: ProtectionLayer.Secure,
            target: BlockTarget.Camera);

        var activeSession = PrivacyRecoveryTestData.ActiveSession(cameraSecurePolicy);
        var store = new RecordingPrivacySessionStore();
        store.Save(activeSession);

        var platform = new ScriptedPrivacySessionPlatformAdapter();
        platform.SetObservation(cameraSecurePolicy.ResourceId, PrivacyResourceObservationKind.MatchesProtected);

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.SetAdvancedProtectionAsync(false);

        Assert.True(result.Success);
        // Restored through WAL platform adapter, NOT through raw native call
        Assert.Contains(platform.RestoredResources, r => r.ResourceId == cameraSecurePolicy.ResourceId);
        var reloaded = store.Load();
        Assert.NotNull(reloaded);
        Assert.Equal(PrivacyResourceJournalState.Restored, reloaded.Resources.First().JournalState);
    }

    // =========================================================================
    // 9. SetAdvancedProtectionAsync(false) with Secure Unknown and without confirmed ownership:
    //    Does not touch external state and reports uncertainty/conflict.
    // =========================================================================
    [Fact]
    public async Task Scenario09_SetAdvancedProtectionFalse_WhenSecureUnknown_WithoutConfirmedOwnership_PreservesExternalState_ReportsConflict()
    {
        var store = new RecordingPrivacySessionStore(); // Empty WAL - no PrivLock ownership
        var platform = new ScriptedPrivacySessionPlatformAdapter();

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.SetAdvancedProtectionAsync(false);

        // Does not restore anything via platform adapter
        Assert.Empty(platform.RestoredResources);
        Assert.Equal(0, deps.SecureDisableCalls);
    }

    // =========================================================================
    // 10. Reconciliation with Standard Unknown: does not apply Standard blindly
    // =========================================================================
    [Fact]
    public async Task Scenario10_ReconcileDesiredProtection_WhenStandardUnknown_DoesNotApplyBlindly()
    {
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter();

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Unknown,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = false,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active
        });

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.ReconcileDesiredProtectionAsync();

        Assert.False(result.Success);
        Assert.Equal(0, deps.StandardEnableCalls);
        var preservedDesired = deps.Load();
        Assert.Equal(StandardProtectionState.Active, preservedDesired.CameraStandard);
    }

    // =========================================================================
    // 11. Reconciliation with Standard verified and Secure Unknown:
    //     Reconciles Standard if needed, but omits Secure.
    // =========================================================================
    [Fact]
    public async Task Scenario11_ReconcileDesiredProtection_WhenStandardVerified_AndSecureUnknown_ReconcilesStandard_OmitsSecure()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.BeforeStandardEnableReturn = () =>
        {
            platform.SetObservation(stdPolicy.ResourceId, PrivacyResourceObservationKind.MatchesProtected);
        };

        deps.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            CameraSecure = SecureProtectionState.Active
        });

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.ReconcileDesiredProtectionAsync();

        // Standard was applied; Secure was omitted with explicit warning
        Assert.Equal(1, deps.StandardEnableCalls);
        Assert.Equal(0, deps.SecureEnableCalls);
        Assert.False(result.Success);
        Assert.Contains("Camera secure protection skipped", result.ErrorMessage);

        // DesiredState is preserved for both
        var preserved = deps.Load();
        Assert.Equal(StandardProtectionState.Active, preserved.CameraStandard);
        Assert.Equal(SecureProtectionState.Active, preserved.CameraSecure);
    }

    // =========================================================================
    // 12. Reconciliation with Standard Active verified and Secure Available verified:
    //     Preserves existing valid behavior (applies Secure).
    // =========================================================================
    [Fact]
    public async Task Scenario12_ReconcileDesiredProtection_WhenStandardActiveVerified_AndSecureAvailableVerified_AppliesSecureNormally()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.EnableSecureHandler = _ =>
        {
            platform.SetObservation(secDev.ResourceId, PrivacyResourceObservationKind.MatchesProtected);
            return OperationResult.Ok();
        };

        deps.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            CameraSecure = SecureProtectionState.Active
        });

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.ReconcileDesiredProtectionAsync();

        Assert.True(result.Success);
        Assert.Equal(0, deps.StandardEnableCalls); // Already active
        Assert.Equal(1, deps.SecureEnableCalls);   // Secure applied cleanly
    }

    // =========================================================================
    // 13. BlockTarget.Both with mixed states:
    //     Validates partial/atomic semantic, never mutates Unknown target.
    // =========================================================================
    [Fact]
    public async Task Scenario13_BlockTargetBoth_WithMixedVerificationStates_MutatesOnlyVerifiedTarget_NeverUnknown()
    {
        var (stdPolicy, secDev) = CreateCamResources();
        var store = new RecordingPrivacySessionStore();
        var platform = new ScriptedPrivacySessionPlatformAdapter
        {
            CaptureFactory = (l, t, op) => l == ProtectionLayer.Standard ? [stdPolicy] : [secDev]
        };

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Unknown,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = false,
                    SecureIsVerified = false,
                    IsVerified = false
                }
            }
        };

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.EnableStandardProtectionAsync(BlockTarget.Both);

        // Rejects before mutation because Microphone is unverified/unknown
        Assert.False(result.Success);
        Assert.Contains("Microphone", result.ErrorMessage);
        Assert.Equal(0, deps.StandardEnableCalls);
    }

    // =========================================================================
    // 14. WAL conflict on one target:
    //     Does not erase DesiredState of conflicted target; other target untouched.
    // =========================================================================
    [Fact]
    public async Task Scenario14_WalConflictOnOneTarget_PreservesDesiredStateOfConflictedTarget_OtherTargetReceivesNoDestructiveChanges()
    {
        var store = new RecordingPrivacySessionStore(); // No session in WAL
        var platform = new ScriptedPrivacySessionPlatformAdapter();

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        deps.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            MicrophoneStandard = StandardProtectionState.Active
        });

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        // User attempts to unblock Microphone, but it conflicts (no PrivLock ownership)
        var result = await service.UnblockDeviceAsync(BlockTarget.Microphone);

        Assert.False(result.Success);
        Assert.Contains("ownership snapshot", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Invariant: DesiredState was NOT wiped for Microphone, and Camera is completely untouched
        var desired = deps.Load();
        Assert.Equal(StandardProtectionState.Active, desired.MicrophoneStandard);
        Assert.Equal(StandardProtectionState.Active, desired.CameraStandard);
        Assert.Equal(0, deps.StandardDisableCalls);
    }

    // =========================================================================
    // 15. Standard Active + Secure Unknown:
    //     User can request WAL restoration and WAL is updated correctly.
    // =========================================================================
    [Fact]
    public async Task Scenario15_StandardActive_SecureUnknown_UserRequestsWalRestoration_RestoresWalOwnedCorrectly()
    {
        var micResource = PrivacyRecoveryTestData.AudioEndpoint("mic-ep-1");
        var activeSession = PrivacyRecoveryTestData.ActiveSession(micResource);
        var store = new RecordingPrivacySessionStore();
        store.Save(activeSession);

        var platform = new ScriptedPrivacySessionPlatformAdapter();
        platform.SetObservation(micResource.ResourceId, PrivacyResourceObservationKind.MatchesProtected);

        var deps = new RecoveryHostDependencies
        {
            CurrentProtectionState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unknown,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = false
                }
            }
        };

        deps.SetDesiredState(new DesiredState
        {
            MicrophoneStandard = StandardProtectionState.Active
        });

        var sessions = new PrivacySessionService(store, platform);
        var service = new ProtectionService(deps, deps, deps, deps, sessions);

        var result = await service.UnblockDeviceAsync(BlockTarget.Microphone);

        Assert.True(result.Success);
        Assert.Contains(platform.RestoredResources, r => r.ResourceId == micResource.ResourceId);

        var updatedSession = store.Load();
        Assert.NotNull(updatedSession);
        Assert.Equal(PrivacyResourceJournalState.Restored, updatedSession.Resources.First().JournalState);
    }
}
