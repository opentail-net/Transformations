# Resampler Fidelity Report

Started: 2026-07-07T12:22:46.4717211+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.69 | 26.96 | 0.0004823 | 32000 | 32000 | 48000 | 14 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.65 | 26.93 | 0.0004858 | 32000 | 32000 | 48000 | 22 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.57 | 26.85 | 0.0004954 | 32000 | 32000 | 48000 | 17 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.53 | 26.81 | 0.0004994 | 32000 | 32000 | 48000 | 12 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.34 | 26.62 | 0.0005223 | 32000 | 32000 | 48000 | 72 |  |
| INFO PASS | cubic | v1 | Experimental Cubic | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.18 | 26.46 | 0.0005417 | 32000 | 32000 | 48000 | 1 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.16 | 26.44 | 0.0005443 | 32000 | 32000 | 48000 | 18 |  |
| INFO PASS | spline | v2 | Spline | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 15.83 | 26.11 | 0.0005874 | 32000 | 32000 | 48000 | 4 |  |
| INFO PASS | linear | v1 | Linear | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 15.76 | 26.03 | 0.0005974 | 32000 | 32000 | 48000 | 0 |  |
| INFO PASS | hybrid | v1 | Hybrid | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 15.76 | 26.03 | 0.0005974 | 32000 | 32000 | 48000 | 0 |  |
| INFO PASS | lanczos | v1 | Experimental Lanczos | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 15.65 | 25.93 | 0.0006124 | 32000 | 32000 | 48000 | 3 |  |
| INFO PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 14.86 | 25.14 | 0.0007348 | 32000 | 32000 | 48000 | 6 |  |
| INFO PASS | sinc | v1 | Sinc | no | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 14.10 | 24.38 | 0.0008747 | 32000 | 32000 | 48000 | 13 |  |
| INFO PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 14.01 | 24.29 | 0.0008925 | 32000 | 32000 | 48000 | 0 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 21.08 | 26.94 | 0.0003505 | 16000 | 16000 | 48000 | 6 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 21.08 | 26.94 | 0.0003505 | 16000 | 16000 | 48000 | 7 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 18.36 | 24.22 | 0.0006562 | 16000 | 16000 | 48000 | 10 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 16.53 | 22.39 | 0.001001 | 16000 | 16000 | 48000 | 10 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 15.24 | 21.09 | 0.001348 | 16000 | 16000 | 48000 | 39 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 14.74 | 20.60 | 0.001511 | 16000 | 16000 | 48000 | 13 |  |
| INFO | linear | v1 | Linear | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 0 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | spline | v2 | Spline | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 2 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | sinc | v1 | Sinc | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 5 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | hybrid | v1 | Hybrid | no | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 0 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 2 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 1 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | cubic | v1 | Experimental Cubic | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 1 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 1.90 | 7.75 | 0.02907 | 16000 | 16000 | 48000 | 0 | SNR 1.90 dB; PSNR 7.75 dB |
| INFO PASS | sinc | v1 | Sinc | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 28 |  |
| INFO PASS | hybrid | v1 | Hybrid | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 32 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 20 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 91.44 | 101.91 | 2.616E-11 | 55125 | 55125 | 60000 | 31 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.48 | 99.95 | 4.116E-11 | 55125 | 55125 | 60000 | 51 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 87.97 | 98.44 | 5.816E-11 | 55125 | 55125 | 60000 | 25 |  |
| INFO PASS | spline | v2 | Spline | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 86.68 | 97.15 | 7.829E-11 | 55125 | 55125 | 60000 | 6 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 76.85 | 87.32 | 7.532E-10 | 55125 | 55125 | 60000 | 37 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 74.64 | 85.11 | 1.253E-09 | 55125 | 55125 | 60000 | 87 |  |
| INFO PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 74.26 | 84.72 | 1.369E-09 | 55124 | 55125 | 59998 | 53 |  |
| INFO PASS | cubic | v1 | Experimental Cubic | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 70.38 | 80.85 | 3.345E-09 | 55125 | 55125 | 60000 | 11 |  |
| INFO PASS | lanczos | v1 | Experimental Lanczos | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 54.03 | 64.50 | 1.444E-07 | 55124 | 55125 | 59998 | 8 |  |
| INFO PASS | linear | v1 | Linear | no | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 47.05 | 57.51 | 7.202E-07 | 55124 | 55125 | 59998 | 0 |  |
| INFO PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 32.03 | 42.49 | 2.289E-05 | 55125 | 55125 | 60000 | 2 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.69 | 87.63 | 7.021E-10 | 36000 | 36000 | 72000 | 28 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.177E-10 | 36000 | 36000 | 72000 | 19 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.18E-10 | 36000 | 36000 | 72000 | 20 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.15 | 87.09 | 7.94E-10 | 36000 | 36000 | 72000 | 60 |  |
| INFO PASS | sinc | v1 | Sinc | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 76.67 | 86.61 | 8.876E-10 | 36000 | 36000 | 72000 | 17 |  |
| INFO PASS | hybrid | v1 | Hybrid | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 76.67 | 86.61 | 8.876E-10 | 36000 | 36000 | 72000 | 16 |  |
| INFO PASS | spline | v2 | Spline | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 75.26 | 85.20 | 1.227E-09 | 36000 | 36000 | 72000 | 5 |  |
| INFO PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 74.54 | 84.48 | 1.447E-09 | 36000 | 36000 | 72000 | 8 |  |
| INFO PASS | lanczos | v1 | Experimental Lanczos | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 74.34 | 84.28 | 1.517E-09 | 36000 | 36000 | 72000 | 8 |  |
| INFO PASS | cubic | v1 | Experimental Cubic | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 73.11 | 83.05 | 2.014E-09 | 36000 | 36000 | 72000 | 12 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 71.26 | 81.20 | 3.08E-09 | 36000 | 36000 | 72000 | 96 |  |
| INFO PASS | linear | v1 | Linear | no | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 52.74 | 62.68 | 2.192E-07 | 36000 | 36000 | 72000 | 0 |  |
| INFO PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 29.17 | 39.11 | 4.989E-05 | 36000 | 36000 | 72000 | 0 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 12.55 | 22.49 | 0.002289 | 36000 | 36000 | 72000 | 10 |  |
| INFO PASS | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | ∞ | ∞ | 0 | 48000 | 48000 | 44100 | 0 |  |
| INFO PASS | sinc | v1 | Sinc | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 14 |  |
| INFO PASS | hybrid | v1 | Hybrid | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 14 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 14 |  |
| INFO PASS | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 42.62 | 57.81 | 6.965E-07 | 48000 | 48000 | 44099 | 6 |  |
| INFO PASS | lanczos | v1 | Experimental Lanczos | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 35.80 | 50.99 | 3.351E-06 | 48000 | 48000 | 44099 | 2 |  |
| INFO PASS | spline | v2 | Spline | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 34.26 | 49.45 | 4.778E-06 | 48000 | 48000 | 44100 | 4 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 33.22 | 48.42 | 6.064E-06 | 48000 | 48000 | 44100 | 24 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 32.23 | 47.43 | 7.613E-06 | 48000 | 48000 | 44100 | 19 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 31.83 | 47.03 | 8.351E-06 | 48000 | 48000 | 44100 | 40 |  |
| INFO PASS | cubic | v1 | Experimental Cubic | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 31.19 | 46.38 | 9.692E-06 | 48000 | 48000 | 44100 | 1 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 30.26 | 45.45 | 1.2E-05 | 48000 | 48000 | 44100 | 65 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 28.05 | 43.24 | 1.998E-05 | 48000 | 48000 | 44100 | 16 |  |
| INFO PASS | linear | v1 | Linear | no | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 27.07 | 42.26 | 2.502E-05 | 48000 | 48000 | 44099 | 0 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.14 | 27.24 | 0.0002699 | 30000 | 30000 | 55125 | 15 |  |
| INFO PASS | sinc_v3 | v3 | Sinc | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.12 | 27.22 | 0.0002716 | 30000 | 30000 | 55125 | 15 |  |
| INFO PASS | halfband_cascade | v1 | HalfBandCascade | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.06 | 27.16 | 0.0002752 | 30000 | 30000 | 55125 | 11 |  |
| INFO PASS | sinc_v2 | v2 | Sinc | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.04 | 27.14 | 0.0002764 | 30000 | 30000 | 55125 | 19 |  |
| INFO PASS | right_wing_sinc | v1 | RightWingSinc | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.91 | 27.01 | 0.000285 | 30000 | 30000 | 55125 | 42 |  |
| INFO PASS | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.73 | 26.82 | 0.0002972 | 30000 | 30000 | 55125 | 16 |  |
| INFO PASS | linear | v1 | Linear | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.14 | 26.24 | 0.0003399 | 30000 | 30000 | 55124 | 0 |  |
| INFO PASS | hybrid | v1 | Hybrid | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.14 | 26.24 | 0.0003399 | 30000 | 30000 | 55124 | 0 |  |
| INFO PASS | cubic | v1 | Experimental Cubic | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.02 | 26.12 | 0.0003499 | 30000 | 30000 | 55125 | 1 |  |
| INFO | spline | v2 | Spline | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 11.65 | 25.75 | 0.0003807 | 30000 | 30000 | 55125 | 3 | SNR 11.65 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 11.24 | 25.34 | 0.0004182 | 30000 | 30000 | 55124 | 4 | SNR 11.24 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 10.93 | 25.03 | 0.0004498 | 30000 | 30000 | 55124 | 3 | SNR 10.93 dB |
| INFO | sinc | v1 | Sinc | no | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 10.42 | 24.52 | 0.0005051 | 30000 | 30000 | 55125 | 12 | SNR 10.42 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 9.60 | 23.70 | 0.0006105 | 30000 | 30000 | 55125 | 0 | SNR 9.60 dB |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.92 | 25.79 | 0.0004475 | 20000 | 20000 | 60000 | 11 | SNR 10.92 dB |
| INFO | sinc_v3 | v3 | Sinc | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.88 | 25.76 | 0.0004509 | 20000 | 20000 | 60000 | 16 | SNR 10.88 dB |
| INFO | sinc_v2 | v2 | Sinc | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.84 | 25.72 | 0.000455 | 20000 | 20000 | 60000 | 10 | SNR 10.84 dB |
| INFO | lagrange_fd | v1 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.75 | 25.62 | 0.0004651 | 20000 | 20000 | 60000 | 26 | SNR 10.75 dB |
| INFO | right_wing_sinc | v1 | RightWingSinc | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.74 | 25.61 | 0.0004657 | 20000 | 20000 | 60000 | 45 | SNR 10.74 dB |
| INFO | halfband_cascade | v1 | HalfBandCascade | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 9.28 | 24.15 | 0.0006519 | 20000 | 20000 | 60000 | 16 | SNR 9.28 dB |
| INFO | linear | v1 | Linear | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 9.12 | 24.00 | 0.000676 | 20000 | 20000 | 60000 | 19 | SNR 9.12 dB |
| INFO | hybrid | v1 | Hybrid | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 9.12 | 24.00 | 0.000676 | 20000 | 20000 | 60000 | 0 | SNR 9.12 dB |
| INFO | cubic | v1 | Experimental Cubic | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 8.69 | 23.56 | 0.0007465 | 20000 | 20000 | 60000 | 2 | SNR 8.69 dB |
| INFO | spline | v2 | Spline | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 8.49 | 23.36 | 0.000782 | 20000 | 20000 | 60000 | 5 | SNR 8.49 dB |
| INFO | lanczos | v1 | Experimental Lanczos | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 8.43 | 23.30 | 0.0007942 | 20000 | 20000 | 60000 | 8 | SNR 8.43 dB |
| INFO | wdl_sinc | v1 | Experimental WDL_Sinc | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 8.20 | 23.07 | 0.0008361 | 20000 | 20000 | 60000 | 5 | SNR 8.20 dB |
| INFO | sinc | v1 | Sinc | no | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 8.01 | 22.88 | 0.0008746 | 20000 | 20000 | 60000 | 18 | SNR 8.01 dB |
| INFO | nearest_neighbor | v1 | Experimental NearestNeighbor | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 7.48 | 22.35 | 0.0009882 | 20000 | 20000 | 60000 | 1 | SNR 7.48 dB |
