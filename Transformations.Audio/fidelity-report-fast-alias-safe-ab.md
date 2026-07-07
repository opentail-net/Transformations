# Resampler Fidelity Report

Started: 2026-07-07T17:06:15.9878952+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 41.51 | 44.52 | 4.328E-06 | 16000 | 16000 | 48000 | 12 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Alias stress 48k to 16k mono | AliasStress | OneWayReference | 38.62 | 41.63 | 8.426E-06 | 16000 | 16000 | 48000 | 5 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.56 | 7.31 | 0.0376 | 22050 | 22050 | 48000 | 34 | MSE 0.0376; SNR 2.56 dB; PSNR 7.31 dB |
| INFO | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Broadband 96k to 44.1k stereo | DeterministicNoise | RoundTrip | 2.42 | 7.18 | 0.03878 | 22050 | 22050 | 48000 | 26 | MSE 0.03878; SNR 2.42 dB; PSNR 7.18 dB |
| PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.34 | 86.26 | 1.148E-09 | 44100 | 44100 | 48000 | 5 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Music 48k to 44.1k stereo | HarmonicBlend | RoundTrip | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 21 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 48.70 | 61.59 | 3.339E-07 | 16000 | 16000 | 48000 | 14 |  |
| PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Speech 48k to 16k mono | HarmonicBlend | RoundTrip | 43.91 | 56.80 | 1.007E-06 | 16000 | 16000 | 48000 | 25 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.54 | 6.53 | 0.1089 | 32000 | 32000 | 48000 | 7 | MSE 0.1089; SNR 3.54 dB; PSNR 6.53 dB |
| INFO | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Sweep 48k to 32k mono | FrequencySweep | RoundTrip | 3.34 | 6.34 | 0.1139 | 32000 | 32000 | 48000 | 4 | MSE 0.1139; SNR 3.34 dB; PSNR 6.34 dB |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 72.27 | 95.50 | 2.284E-10 | 48000 | 48000 | 44100 | 19 |  |
| PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Transient 44.1k to 48k stereo | TransientPulse | RoundTrip | 28.49 | 51.71 | 5.457E-06 | 48000 | 48000 | 44100 | 5 |  |
| PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 59.03 | 71.92 | 3.094E-08 | 48000 | 48000 | 16000 | 7 |  |
| PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Upsample 16k to 48k mono | HarmonicBlend | RoundTrip | 47.33 | 60.22 | 4.579E-07 | 48000 | 48000 | 16000 | 4 |  |
