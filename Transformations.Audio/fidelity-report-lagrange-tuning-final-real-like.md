# Resampler Fidelity Report

Started: 2026-07-07T17:14:02.9759589+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.69 | 26.96 | 0.0004823 | 32000 | 32000 | 48000 | 14 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.43 | 26.71 | 0.0005118 | 32000 | 32000 | 48000 | 17 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.41 | 26.69 | 0.0005134 | 32000 | 32000 | 48000 | 21 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.39 | 26.67 | 0.0005162 | 32000 | 32000 | 48000 | 18 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 24.05 | 29.90 | 0.0001772 | 16000 | 16000 | 48000 | 17 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 21.08 | 26.94 | 0.0003505 | 16000 | 16000 | 48000 | 8 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 19.03 | 24.89 | 0.0005621 | 16000 | 16000 | 48000 | 13 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 14.74 | 20.60 | 0.001511 | 16000 | 16000 | 48000 | 14 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 22 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 93.47 | 103.94 | 1.64E-11 | 55125 | 55125 | 60000 | 25 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.73 | 100.19 | 3.886E-11 | 55125 | 55125 | 60000 | 21 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.72 | 100.19 | 3.887E-11 | 55125 | 55125 | 60000 | 27 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.27 | 88.21 | 6.136E-10 | 36000 | 36000 | 72000 | 30 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.23 | 88.17 | 6.197E-10 | 36000 | 36000 | 72000 | 35 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.08 | 88.02 | 6.415E-10 | 36000 | 36000 | 72000 | 27 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.18E-10 | 36000 | 36000 | 72000 | 103 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 121 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.71 | 44.90 | 1.362E-05 | 48000 | 48000 | 44100 | 27 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.63 | 44.82 | 1.388E-05 | 48000 | 48000 | 44100 | 21 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.62 | 44.81 | 1.39E-05 | 48000 | 48000 | 44100 | 21 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.14 | 27.24 | 0.0002699 | 30000 | 30000 | 55125 | 15 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.93 | 27.03 | 0.0002835 | 30000 | 30000 | 55125 | 16 |  |
| INFO PASS | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.92 | 27.02 | 0.0002842 | 30000 | 30000 | 55125 | 21 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.91 | 27.01 | 0.0002847 | 30000 | 30000 | 55125 | 17 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.92 | 25.79 | 0.0004475 | 20000 | 20000 | 60000 | 15 | SNR 10.92 dB |
| INFO | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.83 | 25.70 | 0.0004566 | 20000 | 20000 | 60000 | 17 | SNR 10.83 dB |
| INFO | lagrange_fd_o6_r098_w48 | v1-o6-r098-w48 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.82 | 25.69 | 0.0004576 | 20000 | 20000 | 60000 | 22 | SNR 10.82 dB |
| INFO | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.79 | 25.66 | 0.000461 | 20000 | 20000 | 60000 | 36 | SNR 10.79 dB |
