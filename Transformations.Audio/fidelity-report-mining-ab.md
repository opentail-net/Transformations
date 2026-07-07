# Resampler Fidelity Report

Started: 2026-07-07T11:27:01.9176021+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | sinc_v2 | v2 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 6 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.62 | 41.63 | 8.426E-06 | 16000 | 16000 | 48000 | 11 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.60 | 41.61 | 8.456E-06 | 16000 | 16000 | 48000 | 34 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 34.27 | 37.28 | 2.289E-05 | 16000 | 16000 | 48000 | 10 |  |
| INFO | sinc | v1 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 8 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 2 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.54 | 7.29 | 0.03776 | 22050 | 22050 | 48000 | 21 | MSE 0.03776; SNR 2.54 dB; PSNR 7.29 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.49 | 7.24 | 0.0382 | 22050 | 22050 | 48000 | 45 | MSE 0.0382; SNR 2.49 dB; PSNR 7.24 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.39 | 7.15 | 0.03904 | 22050 | 22050 | 48000 | 71 | MSE 0.03904; SNR 2.39 dB; PSNR 7.15 dB |
| INFO | halfband_cascade | v1 | HalfBandCascade | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 1.95 | 6.70 | 0.04328 | 22050 | 22050 | 48000 | 22 | MSE 0.04328; SNR 1.95 dB; PSNR 6.70 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 0.10 | 4.86 | 0.06619 | 22049 | 22050 | 47997 | 5 | MSE 0.06619; SNR 0.10 dB; PSNR 4.86 dB |
| INFO | sinc | v1 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.26 | 4.50 | 0.07185 | 22050 | 22050 | 48000 | 90 | MSE 0.07185; SNR -0.26 dB; PSNR 4.50 dB |
| PASS | sinc | v1 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 23 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 70.46 | 83.38 | 2.228E-09 | 44100 | 44100 | 48000 | 28 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.95 | 82.87 | 2.504E-09 | 44100 | 44100 | 48000 | 20 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.76 | 82.68 | 2.616E-09 | 44100 | 44100 | 48000 | 39 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 64.45 | 77.38 | 8.875E-09 | 44099 | 44100 | 47998 | 6 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 58.33 | 71.25 | 3.639E-08 | 44100 | 44100 | 48000 | 66 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 49.22 | 62.12 | 2.959E-07 | 16000 | 16000 | 48000 | 36 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.75 | 61.64 | 3.303E-07 | 16000 | 16000 | 48000 | 8 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.71 | 61.60 | 3.334E-07 | 16000 | 16000 | 48000 | 13 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.68 | 60.57 | 4.227E-07 | 16000 | 16000 | 48000 | 3 |  |
| PASS | sinc | v1 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.65 | 60.54 | 4.255E-07 | 16000 | 16000 | 48000 | 29 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 13.79 | 26.68 | 0.001034 | 16000 | 16000 | 48000 | 12 |  |
| INFO | sinc_v3 | v3 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.51 | 6.51 | 0.1094 | 32000 | 32000 | 48000 | 14 | MSE 0.1094; SNR 3.51 dB; PSNR 6.51 dB |
| INFO | halfband_cascade | v1 | HalfBandCascade | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.45 | 6.45 | 0.1109 | 32000 | 32000 | 48000 | 12 | MSE 0.1109; SNR 3.45 dB; PSNR 6.45 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.43 | 6.43 | 0.1116 | 32000 | 32000 | 48000 | 7 | MSE 0.1116; SNR 3.43 dB; PSNR 6.43 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.29 | 6.29 | 0.1153 | 32000 | 32000 | 48000 | 36 | MSE 0.1153; SNR 3.29 dB; PSNR 6.29 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 1.60 | 4.60 | 0.1699 | 32000 | 32000 | 48000 | 4 | MSE 0.1699; SNR 1.60 dB; PSNR 4.60 dB |
| INFO | sinc | v1 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 0.86 | 3.86 | 0.2015 | 32000 | 32000 | 48000 | 8 | MSE 0.2015; SNR 0.86 dB; PSNR 3.86 dB |
| PASS | sinc | v1 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 25 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 37.89 | 61.11 | 6.267E-07 | 48000 | 48000 | 44099 | 8 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.52 | 51.75 | 5.418E-06 | 48000 | 48000 | 44100 | 24 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 27.54 | 50.76 | 6.794E-06 | 48000 | 48000 | 44100 | 22 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 27.13 | 50.36 | 7.462E-06 | 48000 | 48000 | 44100 | 42 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 25.45 | 48.67 | 1.1E-05 | 48000 | 48000 | 44100 | 72 |  |
| PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 375.06 | 387.95 | 7.726E-40 | 48000 | 48000 | 16000 | 2 |  |
| PASS | sinc | v1 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 360.23 | 373.12 | 2.347E-38 | 48000 | 48000 | 16000 | 7 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 58.12 | 71.01 | 3.816E-08 | 48000 | 48000 | 16000 | 8 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 57.29 | 70.18 | 4.619E-08 | 48000 | 48000 | 16000 | 6 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 51.66 | 64.56 | 1.688E-07 | 48000 | 48000 | 16000 | 33 |  |
| PASS | halfband_cascade | v1 | HalfBandCascade | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 15.55 | 28.45 | 0.0006891 | 48000 | 48000 | 16000 | 10 |  |
