# Resampler Fidelity Report

Started: 2026-07-07T11:58:20.8084939+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | sinc_v2 | v2 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 6 |  |
| INFO PASS | hybrid_v2 | v2 | Hybrid | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 7 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 8 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.60 | 41.61 | 8.456E-06 | 16000 | 16000 | 48000 | 35 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 34.27 | 37.28 | 2.289E-05 | 16000 | 16000 | 48000 | 10 |  |
| INFO PASS | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 22.80 | 25.81 | 0.0003212 | 16000 | 16000 | 48000 | 14 |  |
| INFO | sinc | v1 | Sinc | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 18 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | hybrid | v1 | Hybrid | no | Alias stress 48k to 16k mono | AliasStress | OneWayReference | -0.00 | 3.01 | 0.06125 | 16000 | 16000 | 48000 | 0 | MSE 0.06125; SNR -0.00 dB; PSNR 3.01 dB |
| INFO | hybrid_v2 | v2 | Hybrid | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.56 | 7.31 | 0.0376 | 22050 | 22050 | 48000 | 27 | MSE 0.0376; SNR 2.56 dB; PSNR 7.31 dB |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.56 | 7.31 | 0.0376 | 22050 | 22050 | 48000 | 30 | MSE 0.0376; SNR 2.56 dB; PSNR 7.31 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.54 | 7.29 | 0.03776 | 22050 | 22050 | 48000 | 22 | MSE 0.03776; SNR 2.54 dB; PSNR 7.29 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.49 | 7.24 | 0.0382 | 22050 | 22050 | 48000 | 49 | MSE 0.0382; SNR 2.49 dB; PSNR 7.24 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.39 | 7.15 | 0.03904 | 22050 | 22050 | 48000 | 72 | MSE 0.03904; SNR 2.39 dB; PSNR 7.15 dB |
| INFO | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.31 | 7.06 | 0.03981 | 22050 | 22050 | 48000 | 30 | MSE 0.03981; SNR 2.31 dB; PSNR 7.06 dB |
| INFO | sinc | v1 | Sinc | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.26 | 4.50 | 0.07185 | 22050 | 22050 | 48000 | 13 | MSE 0.07185; SNR -0.26 dB; PSNR 4.50 dB |
| INFO | hybrid | v1 | Hybrid | no | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | -0.26 | 4.50 | 0.07185 | 22050 | 22050 | 48000 | 13 | MSE 0.07185; SNR -0.26 dB; PSNR 4.50 dB |
| PASS | sinc | v1 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 25 |  |
| PASS | hybrid | v1 | Hybrid | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 21 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 25 |  |
| PASS | hybrid_v2 | v2 | Hybrid | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 70.85 | 83.77 | 2.035E-09 | 44100 | 44100 | 48000 | 30 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 70.46 | 83.38 | 2.228E-09 | 44100 | 44100 | 48000 | 32 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.76 | 82.68 | 2.616E-09 | 44100 | 44100 | 48000 | 47 |  |
| PASS | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 67.81 | 80.73 | 4.101E-09 | 44100 | 44100 | 48000 | 39 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 58.33 | 71.25 | 3.639E-08 | 44100 | 44100 | 48000 | 74 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 49.22 | 62.12 | 2.959E-07 | 16000 | 16000 | 48000 | 35 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.75 | 61.64 | 3.303E-07 | 16000 | 16000 | 48000 | 10 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.71 | 61.60 | 3.334E-07 | 16000 | 16000 | 48000 | 14 |  |
| PASS | hybrid_v2 | v2 | Hybrid | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.70 | 61.59 | 3.339E-07 | 16000 | 16000 | 48000 | 10 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.70 | 61.59 | 3.339E-07 | 16000 | 16000 | 48000 | 8 |  |
| PASS | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.66 | 61.55 | 3.37E-07 | 16000 | 16000 | 48000 | 12 |  |
| PASS | sinc | v1 | Sinc | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.65 | 60.54 | 4.255E-07 | 16000 | 16000 | 48000 | 33 |  |
| PASS | hybrid | v1 | Hybrid | no | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 21.35 | 34.24 | 0.0001816 | 16000 | 16000 | 48000 | 1 |  |
| INFO | hybrid_v2 | v2 | Hybrid | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.54 | 6.53 | 0.1089 | 32000 | 32000 | 48000 | 7 | MSE 0.1089; SNR 3.54 dB; PSNR 6.53 dB |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.54 | 6.53 | 0.1089 | 32000 | 32000 | 48000 | 8 | MSE 0.1089; SNR 3.54 dB; PSNR 6.53 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.51 | 6.51 | 0.1094 | 32000 | 32000 | 48000 | 13 | MSE 0.1094; SNR 3.51 dB; PSNR 6.51 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.43 | 6.43 | 0.1116 | 32000 | 32000 | 48000 | 6 | MSE 0.1116; SNR 3.43 dB; PSNR 6.43 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.29 | 6.29 | 0.1153 | 32000 | 32000 | 48000 | 38 | MSE 0.1153; SNR 3.29 dB; PSNR 6.29 dB |
| INFO | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.16 | 6.16 | 0.1187 | 32000 | 32000 | 48000 | 17 | MSE 0.1187; SNR 3.16 dB; PSNR 6.16 dB |
| INFO | hybrid | v1 | Hybrid | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 2.71 | 5.71 | 0.1317 | 32000 | 32000 | 48000 | 0 | MSE 0.1317; SNR 2.71 dB; PSNR 5.71 dB |
| INFO | sinc | v1 | Sinc | no | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 0.86 | 3.86 | 0.2015 | 32000 | 32000 | 48000 | 7 | MSE 0.2015; SNR 0.86 dB; PSNR 3.86 dB |
| PASS | sinc | v1 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 15 |  |
| PASS | hybrid | v1 | Hybrid | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 16 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 15 |  |
| PASS | hybrid_v2 | v2 | Hybrid | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.85 | 52.07 | 5.027E-06 | 48000 | 48000 | 44100 | 30 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.52 | 51.75 | 5.418E-06 | 48000 | 48000 | 44100 | 25 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 27.13 | 50.36 | 7.462E-06 | 48000 | 48000 | 44100 | 42 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 25.45 | 48.67 | 1.1E-05 | 48000 | 48000 | 44100 | 74 |  |
| PASS | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 24.41 | 47.63 | 1.399E-05 | 48000 | 48000 | 44100 | 38 |  |
| PASS | hybrid | v1 | Hybrid | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | sinc | v1 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 360.23 | 373.12 | 2.347E-38 | 48000 | 48000 | 16000 | 5 |  |
| PASS | hybrid_v2 | v2 | Hybrid | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 59.03 | 71.92 | 3.094E-08 | 48000 | 48000 | 16000 | 6 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 59.03 | 71.92 | 3.094E-08 | 48000 | 48000 | 16000 | 8 |  |
| PASS | sinc_v3 | v3 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 58.12 | 71.01 | 3.816E-08 | 48000 | 48000 | 16000 | 11 |  |
| PASS | sinc_v2 | v2 | Sinc | no | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 57.29 | 70.18 | 4.619E-08 | 48000 | 48000 | 16000 | 6 |  |
| PASS | adaptive_kaiser_sinc | v1 | Experimental AdaptiveKaiserSinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 55.36 | 68.25 | 7.207E-08 | 48000 | 48000 | 16000 | 14 |  |
| PASS | right_wing_sinc | v1 | RightWingSinc | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 51.66 | 64.56 | 1.688E-07 | 48000 | 48000 | 16000 | 36 |  |
