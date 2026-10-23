const Q=[{n:"Làm ngay",e:"🔥",d:"Gấp + quan trọng"},{n:"Lên kế hoạch",e:"🌱",d:"Quan trọng, chưa gấp"},{n:"Xong nhanh rồi thôi",e:"⚡",d:"Có vẻ gấp, không quan trọng"},{n:"Bỏ qua / sau cũng được",e:"🗑️",d:"Không gấp, không quan trọng"}];
const N=10,seen={};
const FB=["Gấp thật và ảnh hưởng trực tiếp đến kết quả học tập.","Quan trọng về lâu dài nhưng chưa gấp, nên lên kế hoạch.","Trông có vẻ gấp nhưng không thật sự quan trọng, xử lý nhanh rồi thôi.","Không gấp và không quan trọng, bỏ qua hoặc để sau cũng được."];
let mode,order,i,score,res,tasks,place,sel=null,checked=false;
const $=s=>document.querySelector(s);
const shuf=a=>{a=[...a];for(let n=a.length;n>1;){const j=Math.random()*n--|0;[a[n],a[j]]=[a[j],a[n]]}return a};
const store={get(k){try{return +localStorage.getItem(k)||0}catch(e){return 0}},set(k,v){try{localStorage.setItem(k,v)}catch(e){}}};
const focus=s=>{const el=s&&$(s);if(el)el.focus({preventScroll:true})};
function menu(){
  $("#meta").textContent="";
  $("#app").innerHTML=`<div class="card"><b>Bạn là ai?</b><p>Mỗi ván gồm ${N} tình huống ngẫu nhiên, lấy từ ${D.length} tình huống.</p><div class="row"><button class="btn" data-m="h">🎒 Học sinh</button><button class="btn" data-m="s">🎓 Sinh viên</button><button class="btn" data-m="all">Tất cả</button></div></div>`;
}
function start(m){
  mode=m;
  const p=D.filter(x=>m==="all"||!x.p||x.p==="a"||x.p===m),s=seen[m]||(seen[m]=new Set());
  let o=shuf(p.filter(x=>!s.has(x)));          // ưu tiên tình huống chưa gặp
  if(o.length<N){s.clear();o=o.concat(shuf(p.filter(x=>!o.includes(x))))}
  order=o.slice(0,N);order.forEach(x=>s.add(x));
  i=0;score=0;res=[];load();
}
function load(){
  tasks=shuf(order[i].t.map(([q,t,w])=>({q:q-1,t,w})));
  place=tasks.map(()=>null);sel=null;checked=false;
  render();window.scrollTo(0,0);focus("#sit");
}
function chip(k){
  const t=tasks[k],ok=checked&&place[k]===t.q,bad=checked&&place[k]!==t.q;
  const hint=bad?`<small>Đúng ra: ${Q[t.q].e} ${Q[t.q].n}</small>`:"";
  return `<div class="task ${sel===k?"sel":""} ${ok?"ok":""} ${bad?"bad":""}" role="button" tabindex="0" draggable="${!checked}" data-k="${k}" aria-pressed="${sel===k}">${t.t}${hint}</div>`;
}
function quad(qi){
  const items=tasks.map((_,k)=>place[k]===qi?chip(k):"").join("");
  return `<div class="quad q${qi} ${sel!==null&&!checked?"hot":""}" data-q="${qi}" role="group" aria-label="${Q[qi].n}"><button class="qb" ${checked?"disabled":""}><b>${Q[qi].e} ${Q[qi].n}</b><span>${Q[qi].d}</span></button>${items}</div>`;
}
function render(f){
  const d=order[i],full=place.every(p=>p!==null);
  $("#meta").textContent=`Tình huống ${i+1}/${order.length} · Điểm ${score}`;
  const pool=tasks.map((_,k)=>place[k]===null?chip(k):"").join("")||(checked?"":'<span class="hint">Đã xếp hết. Bấm Kiểm tra khi sẵn sàng.</span>');
  const fb=checked?`<div class="card fb" role="status"><b>${place.filter((p,k)=>p===tasks[k].q).length}/4 việc xếp đúng</b><ul class="why">${tasks.map((t,k)=>{const ok=place[k]===t.q;return `<li class="${ok?"ok":"bad"}"><b>${ok?"✓":"✗"} ${t.t}</b><span>${ok?"":`Bạn xếp vào ${Q[place[k]].e} ${Q[place[k]].n}. `}Đúng là ${Q[t.q].e} ${Q[t.q].n}: ${t.w||FB[t.q]}</span></li>`}).join("")}</ul>${d.tip?`<p>💡 ${d.tip}</p>`:""}</div>`:"";
  const hint=checked?"":sel!==null?"Giờ chạm vào một ô để xếp việc đã chọn (Esc để bỏ chọn).":"Kéo việc vào ô, hoặc chạm một việc rồi chạm vào ô muốn xếp. Chạm lại việc đã xếp để bỏ ra.";
  $("#app").innerHTML=`<div class="bar"><i style="width:${(i+(checked?1:0))/order.length*100}%"></i></div>
    <div class="card" id="sit" tabindex="-1">${d.s}</div>
    <p class="hint">${hint}</p>
    <div class="pool">${pool}</div>
    <div class="grid">
      <div></div><div class="ax">Gấp</div><div class="ax">Chưa gấp</div>
      <div class="ax v">Quan trọng</div>${quad(0)}${quad(1)}
      <div class="ax v">Không quan trọng</div>${quad(2)}${quad(3)}
    </div>${fb}
    <div class="row">${checked?`<button class="btn" id="next">${i+1<order.length?"Tình huống tiếp":"Xem kết quả"}</button>`:`<button class="btn" id="check" ${full?"":"disabled"}>Kiểm tra</button>`}</div>`;
  focus(f);
}
function end(){
  const max=order.length*4,p=Math.round(score/max*100),key="4ov-best-"+mode,best=Math.max(store.get(key),score);
  store.set(key,best);
  const msg=p>=85?"Bạn quản lý ưu tiên rất tốt.":p>=60?"Khá ổn. Hãy xem lại những ô bạn hay nhầm.":"Chơi lại để luyện phân biệt gấp và quan trọng nhé.";
  window.SoftSyncGameHistory?.append('eisenhower',{
    title:`Eisenhower Matrix · ${mode==='h'?'Học sinh':mode==='s'?'Sinh viên':'Tất cả'}`,
    score,unit:`/${max}`,summary:msg,
    metrics:[{label:'Tình huống',value:order.length},{label:'Điểm tối đa',value:max},{label:'Điểm cao nhất',value:`${best}/${max}`}],
    details:order.map((scenario,index)=>`${index+1}. ${scenario.s} — ${res[index]||0}/4 việc xếp đúng`)
  });
  $("#meta").textContent="Kết thúc";
  $("#app").innerHTML=`<div class="card"><p class="big">${score}/${max}</p><p>${msg}</p><p class="meta">Điểm cao nhất của nhóm này trên máy bạn: ${best}/${max}</p><div class="dots">${res.map((c,n)=>`<span>${n+1}: ${c}/4</span>`).join("")}</div><div class="row"><button class="btn" id="again">Chơi lại với 10 tình huống mới</button><button class="btn" id="menu">Đổi nhóm</button></div></div>`;
  window.scrollTo(0,0);focus("#again");
}
$("#app").addEventListener("click",e=>{
  const g=s=>e.target.closest(s);
  if(g("#check")){const c=place.filter((p,k)=>p===tasks[k].q).length;checked=true;score+=c;res.push(c);render("#next");$(".fb").scrollIntoView({block:"nearest"});return}
  if(g("#next")){i++;i<order.length?load():end();return}
  if(g("#again")){start(mode);return}
  if(g("#menu")){menu();return}
  if(g("[data-m]")){start(g("[data-m]").dataset.m);return}
  if(checked)return;
  const t=g(".task"),q=g(".quad");
  if(t){
    const k=+t.dataset.k;
    if(sel!==null&&place[k]!==null){const a=sel;place[a]=place[k];sel=null;render(`[data-k="${a}"]`);return} // đang chọn 1 việc, chạm việc đã xếp: xếp vào cùng ô
    if(place[k]!==null){place[k]=null;sel=null}else sel=sel===k?null:k;
    render(`[data-k="${k}"]`);return;
  }
  if(q&&sel!==null){const a=sel;place[a]=+q.dataset.q;sel=null;render(`[data-k="${a}"]`)}
});
document.addEventListener("keydown",e=>{
  if(e.key==="Escape"&&sel!==null&&!checked){sel=null;render()}
  if((e.key==="Enter"||e.key===" ")&&e.target.classList&&e.target.classList.contains("task")){e.preventDefault();e.target.click()}
});
// Kéo-thả (máy tính). Trên điện thoại vẫn dùng cách chạm-chọn-chạm-xếp.
let drag=null;
const A=$("#app"),zone=e=>e.target.closest&&e.target.closest(".quad,.pool");
A.addEventListener("dragstart",e=>{const t=e.target.closest(".task");if(!t||checked){e.preventDefault();return}drag=+t.dataset.k;sel=null;e.dataTransfer.setData("text/plain",String(drag));e.dataTransfer.effectAllowed="move";t.classList.add("drag")});
A.addEventListener("dragover",e=>{const z=zone(e);if(z&&drag!==null){e.preventDefault();z.classList.add("over")}});
A.addEventListener("dragleave",e=>{const z=zone(e);if(z&&!z.contains(e.relatedTarget))z.classList.remove("over")});
A.addEventListener("drop",e=>{const z=zone(e);if(!z||drag===null)return;e.preventDefault();const k=drag;drag=null;place[k]=z.classList.contains("quad")?+z.dataset.q:null;render(`[data-k="${k}"]`)});
A.addEventListener("dragend",()=>{drag=null;document.querySelectorAll(".over,.drag").forEach(el=>el.classList.remove("over","drag"))});
menu();
window.SoftSyncGameHistory?.mount({gameKey:'eisenhower',storageKey:'softsync.eisenhower.history.v1',title:'Eisenhower Matrix'});
