# Resampler Fidelity Report

Started: 2026-07-07T17:07:16.0782227+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 6 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 32.31 | 35.32 | 3.599E-05 | 16000 | 16000 | 48000 | 12 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 32.31 | 35.32 | 3.599E-05 | 16000 | 16000 | 48000 | 12 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 32.31 | 35.32 | 3.599E-05 | 16000 | 16000 | 48000 | 15 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.56 | 7.31 | 0.0376 | 22050 | 22050 | 48000 | 32 | MSE 0.0376; SNR 2.56 dB; PSNR 7.31 dB |
| INFO | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.43 | 7.19 | 0.0387 | 22050 | 22050 | 48000 | 17 | MSE 0.0387; SNR 2.43 dB; PSNR 7.19 dB |
| INFO | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.40 | 7.16 | 0.03897 | 22050 | 22050 | 48000 | 16 | MSE 0.03897; SNR 2.40 dB; PSNR 7.16 dB |
| INFO | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.34 | 7.10 | 0.03948 | 22050 | 22050 | 48000 | 15 | MSE 0.03948; SNR 2.34 dB; PSNR 7.10 dB |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 19 |  |
| PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 72.92 | 85.84 | 1.263E-09 | 44100 | 44100 | 48000 | 19 |  |
| PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 72.85 | 85.77 | 1.284E-09 | 44100 | 44100 | 48000 | 17 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 69.80 | 82.72 | 2.59E-09 | 44100 | 44100 | 48000 | 15 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.70 | 61.59 | 3.339E-07 | 16000 | 16000 | 48000 | 13 |  |
| PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 47.31 | 60.20 | 4.596E-07 | 16000 | 16000 | 48000 | 14 |  |
| PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 43.03 | 55.92 | 1.232E-06 | 16000 | 16000 | 48000 | 13 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 35.24 | 48.13 | 7.41E-06 | 16000 | 16000 | 48000 | 31 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.54 | 6.53 | 0.1089 | 32000 | 32000 | 48000 | 10 | MSE 0.1089; SNR 3.54 dB; PSNR 6.53 dB |
| INFO | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.32 | 6.32 | 0.1143 | 32000 | 32000 | 48000 | 21 | MSE 0.1143; SNR 3.32 dB; PSNR 6.32 dB |
| INFO | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.26 | 6.26 | 0.1159 | 32000 | 32000 | 48000 | 16 | MSE 0.1159; SNR 3.26 dB; PSNR 6.26 dB |
| INFO | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.15 | 6.15 | 0.1189 | 32000 | 32000 | 48000 | 12 | MSE 0.1189; SNR 3.15 dB; PSNR 6.15 dB |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 20 |  |
| PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 24.90 | 48.13 | 1.247E-05 | 48000 | 48000 | 44100 | 25 |  |
| PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 24.20 | 47.43 | 1.465E-05 | 48000 | 48000 | 44100 | 20 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 23.11 | 46.33 | 1.886E-05 | 48000 | 48000 | 44100 | 23 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 59.03 | 71.92 | 3.094E-08 | 48000 | 48000 | 16000 | 7 |  |
| PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 53.02 | 65.91 | 1.236E-07 | 48000 | 48000 | 16000 | 15 |  |
| PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 46.01 | 58.90 | 6.206E-07 | 48000 | 48000 | 16000 | 13 |  |
| PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 37.32 | 50.21 | 4.586E-06 | 48000 | 48000 | 16000 | 11 |  |
