let state;

export async function startRecording() {
  const stream = await navigator.mediaDevices.getUserMedia({ audio: { echoCancellation: true, noiseSuppression: true } });
  const context = new AudioContext();
  const source = context.createMediaStreamSource(stream);
  const processor = context.createScriptProcessor(2048, 1, 1);
  const samples = [];
  processor.onaudioprocess = e => samples.push(new Float32Array(e.inputBuffer.getChannelData(0)));
  source.connect(processor); processor.connect(context.destination);
  const chunks = [];
  const recorder = new MediaRecorder(stream);
  recorder.ondataavailable = e => { if (e.data.size) chunks.push(e.data); };
  let transcript = "", confidence = 0, recognition;
  const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (SpeechRecognition) {
    recognition = new SpeechRecognition(); recognition.lang = "vi-VN"; recognition.continuous = true; recognition.interimResults = false;
    recognition.onresult = e => { for (let i=e.resultIndex;i<e.results.length;i++) if(e.results[i].isFinal){ transcript += " " + e.results[i][0].transcript; confidence = Math.max(confidence, e.results[i][0].confidence || 0); } };
    try { recognition.start(); } catch {}
  }
  recorder.start(); state = { stream, context, processor, recorder, chunks, samples, started: performance.now(), getTranscript:()=>transcript.trim(), getConfidence:()=>confidence, recognition };
}

export async function stopRecording() {
  if (!state) throw new Error("Chưa bắt đầu ghi âm");
  const s = state;
  await new Promise(resolve => { s.recorder.onstop = resolve; s.recorder.stop(); });
  try { s.recognition?.stop(); } catch {}
  await new Promise(r => setTimeout(r, 250));
  s.processor.disconnect(); s.stream.getTracks().forEach(t=>t.stop());
  const duration = (performance.now()-s.started)/1000;
  const data = merge(s.samples), sampleRate=s.context.sampleRate;
  const metrics = analyse(data, sampleRate);
  const blob = new Blob(s.chunks, {type:s.recorder.mimeType});
  const audioUrl = URL.createObjectURL(blob);
  await s.context.close(); state=null;
  return { duration, transcript:s.getTranscript(), confidence:s.getConfidence(), audioUrl, ...metrics };
}

function merge(parts){ const n=parts.reduce((a,x)=>a+x.length,0), out=new Float32Array(n); let p=0; for(const x of parts){out.set(x,p);p+=x.length;} return out; }
function analyse(x,sr){
  const frame=Math.round(sr*.03), hop=Math.round(sr*.015), rms=[], pitches=[]; let silent=0,maxSilent=0,current=0;
  for(let p=0;p+frame<x.length;p+=hop){ let sum=0; for(let i=0;i<frame;i++)sum+=x[p+i]*x[p+i]; const r=Math.sqrt(sum/frame); rms.push(r); if(r<.012){current+=hop/sr;maxSilent=Math.max(maxSilent,current)}else{if(current>.5)silent+=current;current=0; const f=pitch(x,p,frame,sr);if(f>70&&f<450)pitches.push(f);} }
  const mean=avg(rms), rmsCv=mean?sd(rms,mean)/mean:0, pm=avg(pitches), pitchSd=pitches.length?sd(pitches,pm):0;
  return { silenceSeconds:silent, longestSilence:maxSilent, rmsVariation:rmsCv, meanPitch:pm||0, pitchSd };
}
function pitch(x,p,n,sr){let best=0,bestLag=0; const lo=Math.floor(sr/450),hi=Math.min(Math.floor(sr/70),n/2);for(let lag=lo;lag<hi;lag++){let c=0;for(let i=0;i<n-lag;i++)c+=x[p+i]*x[p+i+lag];if(c>best){best=c;bestLag=lag}}return bestLag?sr/bestLag:0}
const avg=a=>a.length?a.reduce((x,y)=>x+y,0)/a.length:0;
const sd=(a,m)=>Math.sqrt(a.reduce((s,v)=>s+(v-m)**2,0)/Math.max(1,a.length-1));
