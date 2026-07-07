# Resampler Fidelity Report

Started: 2026-07-07T17:06:44.2904324+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.69 | 26.96 | 0.0004823 | 32000 | 32000 | 48000 | 28 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture field 48k to 32k stereo | FieldRecordingFixture | RoundTrip | 16.69 | 26.96 | 0.0004823 | 32000 | 32000 | 48000 | 20 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 21.08 | 26.94 | 0.0003505 | 16000 | 16000 | 48000 | 11 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture high-frequency 48k to 16k mono | HighFrequencyFixture | OneWayReference | 18.36 | 24.22 | 0.0006562 | 16000 | 16000 | 48000 | 11 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 24 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 48k to 44.1k stereo | MusicFixture | RoundTrip | 94.01 | 104.48 | 1.45E-11 | 55125 | 55125 | 60000 | 22 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.18E-10 | 36000 | 36000 | 72000 | 100 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture music 96k to 48k stereo | MusicFixture | RoundTrip | 77.59 | 87.53 | 7.18E-10 | 36000 | 36000 | 72000 | 100 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 118 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture percussion 44.1k to 48k stereo | PercussionFixture | RoundTrip | 79.25 | 94.45 | 1.513E-10 | 48000 | 48000 | 44100 | 118 |  |
| INFO PASS | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.14 | 27.24 | 0.0002699 | 30000 | 30000 | 55125 | 15 |  |
| INFO PASS | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 44.1k to 24k mono | SpeechFixture | RoundTrip | 13.14 | 27.24 | 0.0002699 | 30000 | 30000 | 55125 | 14 |  |
| INFO | ratio_aware_sinc_hybrid | v1 | Experimental RatioAwareSincHybrid | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.92 | 25.79 | 0.0004475 | 20000 | 20000 | 60000 | 12 | SNR 10.92 dB |
| INFO | fast_alias_safe | v1 | Experimental FastAliasSafe | yes | Fixture speech 48k to 16k mono | SpeechFixture | RoundTrip | 10.74 | 25.61 | 0.0004657 | 20000 | 20000 | 60000 | 37 | SNR 10.74 dB |
