---
name: 'Feature request : ECU definition'
about: Add an ECU with a known DLL/SMR-D that isn't currently in UnlockECU
title: ''
labels: ''
assignees: ''

---

**ECU Name**
Name of the target, e.g. `CRD3NFZ`

**Source file**
Name of the source file where the algorithm and definition can be reverse-engineered from, e.g. `CRD3NFZ_crd3s2_sec9A_12_34_01.dll`

Without a known source file, the issue will be closed as it cannot be worked on. Please note that many SMR-D files do *not* include unlocking functionality. If you are referencing a SMR-D file, ensure that the file has unlocking capabilities first.

**Additional context**
Add any other optional context about the feature request here, such as security levels, expected seed-key pairs etc.

**Checklist**
_Please delete either YES or NO depending on the checklist question_

I have checked that the requested definition does not exist in [db.json](https://github.com/jglim/UnlockECU/blob/main/UnlockECU/db.json) : [YES/NO]
_Some ECUs share a common definition, for example, the CRD3 family uses the CRD3NFZ definition, and are linked together with an [alias](https://github.com/jglim/UnlockECU/blob/bcb77faa30da1babce0aca79a2d63c99618226c2/UnlockECU/db.json#L2019). If an alias is missing, please use the "Bug Report" template instead._
