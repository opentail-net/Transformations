# Resampler Fidelity Report

Started: 2026-07-07T17:13:43.0175981+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.69 | 26.96 | 0.0004823 | 32000 | 32000 | 48000 | 12 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.43 | 26.71 | 0.0005118 | 32000 | 32000 | 48000 | 17 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.39 | 26.67 | 0.0005162 | 32000 | 32000 | 48000 | 18 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.31 | 26.59 | 0.0005256 | 32000 | 32000 | 48000 | 16 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.29 | 26.57 | 0.0005277 | 32000 | 32000 | 48000 | 27 |  |
| INFO PASS | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.09 | 26.37 | 0.0005532 | 32000 | 32000 | 48000 | 16 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 21.08 | 26.94 | 0.0003505 | 16000 | 16000 | 48000 | 6 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 19.03 | 24.89 | 0.0005621 | 16000 | 16000 | 48000 | 13 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 17.52 | 23.38 | 0.0007961 | 16000 | 16000 | 48000 | 18 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 14.74 | 20.60 | 0.001511 | 16000 | 16000 | 48000 | 13 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 14.74 | 20.60 | 0.001511 | 16000 | 16000 | 48000 | 14 |  |
| INFO | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 9.49 | 15.35 | 0.005056 | 16000 | 16000 | 48000 | 12 | SNR 9.49 dB; PSNR 15.35 dB |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 145 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 93.47 | 103.94 | 1.64E-11 | 55125 | 55125 | 60000 | 24 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.73 | 100.19 | 3.886E-11 | 55125 | 55125 | 60000 | 28 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.31 | 99.78 | 4.279E-11 | 55125 | 55125 | 60000 | 27 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 89.29 | 99.76 | 4.293E-11 | 55125 | 55125 | 60000 | 21 |  |
| INFO PASS | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 88.62 | 99.09 | 5.008E-11 | 55125 | 55125 | 60000 | 23 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.23 | 88.17 | 6.193E-10 | 36000 | 36000 | 72000 | 30 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.23 | 88.17 | 6.197E-10 | 36000 | 36000 | 72000 | 24 |  |
| INFO PASS | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.16 | 88.09 | 6.302E-10 | 36000 | 36000 | 72000 | 22 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.14 | 88.08 | 6.318E-10 | 36000 | 36000 | 72000 | 26 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 78.08 | 88.02 | 6.415E-10 | 36000 | 36000 | 72000 | 32 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.18E-10 | 36000 | 36000 | 72000 | 18 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 12 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.71 | 44.90 | 1.362E-05 | 48000 | 48000 | 44100 | 19 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.63 | 44.82 | 1.388E-05 | 48000 | 48000 | 44100 | 17 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.05 | 44.24 | 1.584E-05 | 48000 | 48000 | 44100 | 17 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 29.03 | 44.22 | 1.592E-05 | 48000 | 48000 | 44100 | 21 |  |
| INFO PASS | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 28.08 | 43.27 | 1.982E-05 | 48000 | 48000 | 44100 | 17 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.14 | 27.24 | 0.0002699 | 30000 | 30000 | 55125 | 14 |  |
| INFO PASS | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.93 | 27.03 | 0.0002835 | 30000 | 30000 | 55125 | 15 |  |
| INFO PASS | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.91 | 27.01 | 0.0002847 | 30000 | 30000 | 55125 | 16 |  |
| INFO PASS | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.85 | 26.95 | 0.0002889 | 30000 | 30000 | 55125 | 14 |  |
| INFO PASS | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.84 | 26.94 | 0.0002897 | 30000 | 30000 | 55125 | 19 |  |
| INFO PASS | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 12.69 | 26.78 | 0.0003 | 30000 | 30000 | 55125 | 14 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.92 | 25.79 | 0.0004475 | 20000 | 20000 | 60000 | 15 | SNR 10.92 dB |
| INFO | lagrange_fd_o6_r098 | v1-o6-r098 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.83 | 25.70 | 0.0004566 | 20000 | 20000 | 60000 | 16 | SNR 10.83 dB |
| INFO | lagrange_fd_o8 | v1-o8 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.79 | 25.66 | 0.000461 | 20000 | 20000 | 60000 | 16 | SNR 10.79 dB |
| INFO | lagrange_fd_o6 | v1-o6 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.77 | 25.64 | 0.0004626 | 20000 | 20000 | 60000 | 33 | SNR 10.77 dB |
| INFO | lagrange_fd_o6_w48 | v1-o6-w48 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.76 | 25.63 | 0.0004637 | 20000 | 20000 | 60000 | 22 | SNR 10.76 dB |
| INFO | lagrange_fd_o6_r090 | v1-o6-r090 | Experimental LagrangeFractionalDelay | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.66 | 25.53 | 0.0004745 | 20000 | 20000 | 60000 | 15 | SNR 10.66 dB |
