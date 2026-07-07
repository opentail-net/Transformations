# Resampler Fidelity Report

Started: 2026-07-07T16:58:22.7932517+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 32.14 | 35.15 | 3.741E-05 | 16000 | 16000 | 48000 | 12 |  |
| INFO | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.27 | 7.02 | 0.04019 | 22050 | 22050 | 48000 | 25 | MSE 0.04019; SNR 2.27 dB; PSNR 7.02 dB |
| PASS | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 51.79 | 64.71 | 1.64E-07 | 44100 | 44100 | 48000 | 25 |  |
| PASS | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 29.90 | 42.79 | 2.534E-05 | 16000 | 16000 | 48000 | 30 |  |
| INFO | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.05 | 6.05 | 0.1217 | 32000 | 32000 | 48000 | 12 | MSE 0.1217; SNR 3.05 dB; PSNR 6.05 dB |
| PASS | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 22.77 | 46.00 | 2.036E-05 | 48000 | 48000 | 44100 | 25 |  |
| PASS | zero_phase_iir_cubic | v1 | Experimental ZeroPhaseIirCubic | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 32.04 | 44.94 | 1.546E-05 | 48000 | 48000 | 16000 | 13 |  |
