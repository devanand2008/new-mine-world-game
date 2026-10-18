import assert from 'node:assert/strict';
import {createHash} from 'node:crypto';

// Parse real persisted files without depending on Unity or assuming one save layout.
export function readWorldSave(bytes) {
  assert.ok(bytes.length>=413,'Save has a complete header');
  assert.ok(createHash('sha256').update(bytes.subarray(0,-32)).digest().equals(bytes.subarray(-32)),'Persisted checksum');
  assert.equal(bytes.readInt32LE(0),0x43464543);
  const version=bytes.readInt32LE(4);assert.ok([1,2,3,4].includes(version),'Supported save version');
  const inventoryOffset=53+(version>=2?4:0)+(version>=3?4:0);
  const countOffset=inventoryOffset+27*12+(version>=2?4*12:0),editCount=bytes.readInt32LE(countOffset);
  const progressionOffset=countOffset+4+editCount*13;
  assert.ok(editCount>=0&&editCount<=250000);assert.equal(bytes.length,progressionOffset+(version>=4?20:0)+32);
  const slots=(offset,n)=>Array.from({length:n},(_,i)=>[0,4,8].map(d=>bytes.readInt32LE(offset+i*12+d)));
  return {version,character:version>=4?bytes.readInt32LE(progressionOffset):0,quest:version>=4?Array.from({length:4},(_,i)=>bytes.readInt32LE(progressionOffset+4+i*4)):[0,0,0,0],generatorVersion:version>=3?bytes.readInt32LE(53):version===1?1:2,
    seed:bytes.readInt32LE(8),mode:bytes[12],x:bytes.readFloatLE(13),y:bytes.readFloatLE(17),z:bytes.readFloatLE(21),yaw:bytes.readFloatLE(25),pitch:bytes.readFloatLE(29),
    selected:bytes.readInt32LE(49),experience:version>=2?bytes.readInt32LE(version>=3?57:53):0,
    inventory:slots(inventoryOffset,27),equipment:version>=2?slots(inventoryOffset+324,4):Array.from({length:4},()=>[0,0,0]),editCount,
    edits:Array.from({length:editCount},(_,i)=>{const offset=countOffset+4+i*13;return [bytes.readInt32LE(offset),bytes.readInt32LE(offset+4),bytes.readInt32LE(offset+8),bytes[offset+12]];})};
}
