# Resampler Fidelity Report

Started: 2026-07-07T12:12:48.1434313+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | sinc_v2 | v2 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 7 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 7 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.62 | 41.63 | 8.426E-06 | 16000 | 16000 | 48000 | 14 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.60 | 41.61 | 8.456E-06 | 16000 | 16000 | 48000 | 37 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 34.27 | 37.28 | 2.289E-05 | 16000 | 16000 | 48000 | 9 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 32.31 | 35.32 | 3.599E-05 | 16000 | 16000 | 48000 | 16 |  |
| INFO | linear | v1 | Linear | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 0 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | spline | v2 | Spline | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 4 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | sinc | v1 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 6 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | hybrid | v1 | Hybrid | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 0 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 3 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 5 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | cubic | v1 | Experimental Cubic | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 1 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 0 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.56 | 7.31 | 0.0376 | 22050 | 22050 | 48000 | 43 | MSE 0.0376; SNR 2.56 dB; PSNR 7.31 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.54 | 7.29 | 0.03776 | 22050 | 22050 | 48000 | 21 | MSE 0.03776; SNR 2.54 dB; PSNR 7.29 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.49 | 7.24 | 0.0382 | 22050 | 22050 | 48000 | 44 | MSE 0.0382; SNR 2.49 dB; PSNR 7.24 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.39 | 7.15 | 0.03904 | 22050 | 22050 | 48000 | 72 | MSE 0.03904; SNR 2.39 dB; PSNR 7.15 dB |
| INFO | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.34 | 7.10 | 0.03948 | 22050 | 22050 | 48000 | 21 | MSE 0.03948; SNR 2.34 dB; PSNR 7.10 dB |
| INFO | halfband_cascade | v1 | HalfBandCascade | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 1.95 | 6.70 | 0.04328 | 22050 | 22050 | 48000 | 23 | MSE 0.04328; SNR 1.95 dB; PSNR 6.70 dB |
| INFO | linear | v1 | Linear | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 1.76 | 6.51 | 0.0452 | 22049 | 22050 | 47997 | 0 | MSE 0.0452; SNR 1.76 dB; PSNR 6.51 dB |
| INFO | cubic | v1 | Experimental Cubic | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 1.22 | 5.97 | 0.05117 | 22050 | 22050 | 48000 | 7 | MSE 0.05117; SNR 1.22 dB; PSNR 5.97 dB |
| INFO | spline | v2 | Spline | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 0.73 | 5.49 | 0.05724 | 22050 | 22050 | 48000 | 5 | MSE 0.05724; SNR 0.73 dB; PSNR 5.49 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 0.32 | 5.07 | 0.06299 | 22049 | 22050 | 47997 | 4 | MSE 0.06299; SNR 0.32 dB; PSNR 5.07 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 0.10 | 4.86 | 0.06619 | 22049 | 22050 | 47997 | 5 | MSE 0.06619; SNR 0.10 dB; PSNR 4.86 dB |
| INFO | sinc | v1 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.26 | 4.50 | 0.07185 | 22050 | 22050 | 48000 | 17 | MSE 0.07185; SNR -0.26 dB; PSNR 4.50 dB |
| INFO | hybrid | v1 | Hybrid | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.26 | 4.50 | 0.07185 | 22050 | 22050 | 48000 | 16 | MSE 0.07185; SNR -0.26 dB; PSNR 4.50 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.33 | 4.43 | 0.07308 | 22050 | 22050 | 48000 | 0 | MSE 0.07308; SNR -0.33 dB; PSNR 4.43 dB |
| PASS | spline | v2 | Spline | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.34 | 86.26 | 1.148E-09 | 44100 | 44100 | 48000 | 4 |  |
| PASS | sinc | v1 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 23 |  |
| PASS | hybrid | v1 | Hybrid | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 26 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 23 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 70.46 | 83.38 | 2.228E-09 | 44100 | 44100 | 48000 | 30 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.95 | 82.87 | 2.504E-09 | 44100 | 44100 | 48000 | 24 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.80 | 82.72 | 2.59E-09 | 44100 | 44100 | 48000 | 18 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.76 | 82.68 | 2.616E-09 | 44100 | 44100 | 48000 | 45 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 64.45 | 77.38 | 8.875E-09 | 44099 | 44100 | 47998 | 7 |  |
| PASS | cubic | v1 | Experimental Cubic | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 61.23 | 74.15 | 1.863E-08 | 44100 | 44100 | 48000 | 2 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 58.33 | 71.25 | 3.639E-08 | 44100 | 44100 | 48000 | 71 |  |
| PASS | lanczos | v1 | Experimental Lanczos | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 46.94 | 59.86 | 5.008E-07 | 44099 | 44100 | 47998 | 5 |  |
| PASS | linear | v1 | Linear | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 33.31 | 46.23 | 1.156E-05 | 44099 | 44100 | 47998 | 0 |  |
| PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 22.64 | 35.56 | 0.0001349 | 44100 | 44100 | 48000 | 1 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 49.22 | 62.12 | 2.959E-07 | 16000 | 16000 | 48000 | 37 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.75 | 61.64 | 3.303E-07 | 16000 | 16000 | 48000 | 9 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.71 | 61.60 | 3.334E-07 | 16000 | 16000 | 48000 | 14 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.70 | 61.59 | 3.339E-07 | 16000 | 16000 | 48000 | 9 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.68 | 60.57 | 4.227E-07 | 16000 | 16000 | 48000 | 3 |  |
| PASS | sinc | v1 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.65 | 60.54 | 4.255E-07 | 16000 | 16000 | 48000 | 13 |  |
| PASS | spline | v2 | Spline | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 43.65 | 56.54 | 1.069E-06 | 16000 | 16000 | 48000 | 5 |  |
| PASS | lanczos | v1 | Experimental Lanczos | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 39.86 | 52.75 | 2.557E-06 | 16000 | 16000 | 48000 | 8 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 35.24 | 48.13 | 7.41E-06 | 16000 | 16000 | 48000 | 19 |  |
| PASS | cubic | v1 | Experimental Cubic | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 31.11 | 44.00 | 1.916E-05 | 16000 | 16000 | 48000 | 2 |  |
| PASS | linear | v1 | Linear | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 21.35 | 34.24 | 0.0001816 | 16000 | 16000 | 48000 | 20 |  |
| PASS | hybrid | v1 | Hybrid | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 21.35 | 34.24 | 0.0001816 | 16000 | 16000 | 48000 | 0 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 13.79 | 26.68 | 0.001034 | 16000 | 16000 | 48000 | 11 |  |
| PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 13.47 | 26.36 | 0.001113 | 16000 | 16000 | 48000 | 0 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.54 | 6.53 | 0.1089 | 32000 | 32000 | 48000 | 11 | MSE 0.1089; SNR 3.54 dB; PSNR 6.53 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.51 | 6.51 | 0.1094 | 32000 | 32000 | 48000 | 12 | MSE 0.1094; SNR 3.51 dB; PSNR 6.51 dB |
| INFO | halfband_cascade | v1 | HalfBandCascade | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.45 | 6.45 | 0.1109 | 32000 | 32000 | 48000 | 10 | MSE 0.1109; SNR 3.45 dB; PSNR 6.45 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.43 | 6.43 | 0.1116 | 32000 | 32000 | 48000 | 7 | MSE 0.1116; SNR 3.43 dB; PSNR 6.43 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.29 | 6.29 | 0.1153 | 32000 | 32000 | 48000 | 37 | MSE 0.1153; SNR 3.29 dB; PSNR 6.29 dB |
| INFO | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.15 | 6.15 | 0.1189 | 32000 | 32000 | 48000 | 14 | MSE 0.1189; SNR 3.15 dB; PSNR 6.15 dB |
| INFO | cubic | v1 | Experimental Cubic | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.88 | 5.88 | 0.1266 | 32000 | 32000 | 48000 | 0 | MSE 0.1266; SNR 2.88 dB; PSNR 5.88 dB |
| INFO | linear | v1 | Linear | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.71 | 5.71 | 0.1317 | 32000 | 32000 | 48000 | 0 | MSE 0.1317; SNR 2.71 dB; PSNR 5.71 dB |
| INFO | hybrid | v1 | Hybrid | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.71 | 5.71 | 0.1317 | 32000 | 32000 | 48000 | 0 | MSE 0.1317; SNR 2.71 dB; PSNR 5.71 dB |
| INFO | spline | v2 | Spline | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.44 | 5.44 | 0.14 | 32000 | 32000 | 48000 | 3 | MSE 0.14; SNR 2.44 dB; PSNR 5.44 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.24 | 5.23 | 0.1469 | 32000 | 32000 | 48000 | 3 | MSE 0.1469; SNR 2.24 dB; PSNR 5.23 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 1.60 | 4.60 | 0.1699 | 32000 | 32000 | 48000 | 3 | MSE 0.1699; SNR 1.60 dB; PSNR 4.60 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 1.23 | 4.23 | 0.185 | 32000 | 32000 | 48000 | 0 | MSE 0.185; SNR 1.23 dB; PSNR 4.23 dB |
| INFO | sinc | v1 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 0.86 | 3.86 | 0.2015 | 32000 | 32000 | 48000 | 10 | MSE 0.2015; SNR 0.86 dB; PSNR 3.86 dB |
| PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 44100 | 0 |  |
| PASS | sinc | v1 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 21 |  |
| PASS | hybrid | v1 | Hybrid | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 55 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 19 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 37.89 | 61.11 | 6.267E-07 | 48000 | 48000 | 44099 | 7 |  |
| PASS | lanczos | v1 | Experimental Lanczos | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 29.91 | 53.13 | 3.941E-06 | 48000 | 48000 | 44099 | 6 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.52 | 51.75 | 5.418E-06 | 48000 | 48000 | 44100 | 23 |  |
| PASS | spline | v2 | Spline | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.49 | 51.71 | 5.457E-06 | 48000 | 48000 | 44100 | 5 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 27.54 | 50.76 | 6.794E-06 | 48000 | 48000 | 44100 | 18 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 27.13 | 50.36 | 7.462E-06 | 48000 | 48000 | 44100 | 38 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 25.45 | 48.67 | 1.1E-05 | 48000 | 48000 | 44100 | 69 |  |
| PASS | cubic | v1 | Experimental Cubic | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 24.99 | 48.21 | 1.222E-05 | 48000 | 48000 | 44100 | 9 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 23.11 | 46.33 | 1.886E-05 | 48000 | 48000 | 44100 | 64 |  |
| PASS | linear | v1 | Linear | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 21.47 | 44.69 | 2.751E-05 | 48000 | 48000 | 44099 | 0 |  |
| PASS | linear | v1 | Linear | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | spline | v2 | Spline | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 3 |  |
| PASS | hybrid | v1 | Hybrid | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | cubic | v1 | Experimental Cubic | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 375.06 | 387.95 | 7.726E-40 | 48000 | 48000 | 16000 | 3 |  |
| PASS | lanczos | v1 | Experimental Lanczos | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 374.35 | 387.25 | 9.08E-40 | 48000 | 48000 | 16000 | 3 |  |
| PASS | sinc | v1 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 360.23 | 373.12 | 2.347E-38 | 48000 | 48000 | 16000 | 9 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 59.03 | 71.92 | 3.094E-08 | 48000 | 48000 | 16000 | 9 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 58.12 | 71.01 | 3.816E-08 | 48000 | 48000 | 16000 | 11 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 57.29 | 70.18 | 4.619E-08 | 48000 | 48000 | 16000 | 6 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 51.66 | 64.56 | 1.688E-07 | 48000 | 48000 | 16000 | 38 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 37.32 | 50.21 | 4.586E-06 | 48000 | 48000 | 16000 | 13 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 15.55 | 28.45 | 0.0006891 | 48000 | 48000 | 16000 | 11 |  |
